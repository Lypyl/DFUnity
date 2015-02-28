// this file is a contribution to Daggerfall Tools For Unity
// Project:         Increased Terrain Distance for Daggerfall Tools For Unity
// Author:          Michael Rauter (a.k.a. _Nystul_ from reddit)
// File Version:	1.0
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)

// original project:
// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2015 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    https://github.com/Interkarma/daggerfall-unity

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Demo;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Manages a world terrain object built from the world height map for increased terrain/view distance
    /// one main objective was to make everything work inside this script (and the shader for texturing) without a need for changes in any other files
    /// this means as a consequence that initialization of resources of other scripts that are used inside the script needs to be finished before any other work can be done.
    /// another objective was that render and loading performance should only be impacted as less as possible. I did some investigations and decided to use one big
    /// unity terrain object for the whole world since it gave the best performance compared with splitting up into several terrains. The terrain has to be created only once
    /// and only needs to be translated to match with the StreamingWorld component.
    /// furthermore when using unity terrain one can use unity's level of detail mechanism for geometry (if needed), and other benefits provided by unity’s terrain system.
    /// a consequence is though that low-detailed terrain geometry is also rendered at the position of the detailed terrain inside the distance from the player defined by TerrainDistance.
    /// to decrease the chance of intersecting geometry terrain heights of the low-detailed world map within TerrainDistance are decreased (this area is called sink area in the script) and the world map is translated down on the y-axis a bit
    /// </summary>
    public class IncreasedTerrainDistance : MonoBehaviour
    {
        #region Fields

        // Streaming World Component
        public StreamingWorld streamingWorld;

        // Local player GPS for tracking player virtual position
        public PlayerGPS playerGPS;

        // secondary camera to prevent floating-point rendering precision problems for huge clipping ranges
        public Camera secondaryCamera;

        // is dfUnity ready?
        bool isReady = false;

        // the height values of the world height map used as input for unity terrain function SetHeights()
        float[,] worldHeights = null;

        int worldMapWidth = MapsFile.MaxMapPixelX - MapsFile.MinMapPixelX;
        int worldMapHeight = MapsFile.MaxMapPixelY - MapsFile.MinMapPixelY;

        // used to track changes of playerGPS x- resp. y-position on the world map (-> a change results in an update of the terrain object's translation)
        int MapPixelX = -1;
        int MapPixelY = -1;

        // used to backup old center x- resp. y-position of sink-area to restore old values which are not sunk (sink area is used to decrease the chance of low-detail world height map geometry intersect detailed geometry in near distance defined by TerrainDistance)
        int backupSinkAreaCenterPosX = -1;
        int backupSinkAreaCenterPosY = -1;

        // unity terrain object which will hold the low-detail world map geometry, set to null initially for lazy creation
        GameObject worldTerrainGameObject = null;

        // instance of dfUnity
        DaggerfallUnity dfUnity;

        #endregion

        #region Properties

        public bool IsReady { get { return ReadyCheck(); } }
        //public bool IsInit { get { return init; } }

        #endregion

        #region Unity

        void Start()
        {
            dfUnity = DaggerfallUnity.Instance;

            while (!dfUnity.Setup()) // ugly workaround - this is needed so all content readers are ready - otherwise some readers might be null
            {
            }
            
            // Check if we have PlayerGPS
            if (!playerGPS)
            {
                DaggerfallUnity.LogMessage("StreamingWorld: Missing PlayerGPS reference.", true);
                if (Application.isEditor)
                    Debug.Break();
                else
                    Application.Quit();
            }
/*
            // set up cameras
            Camera.main.clearFlags = CameraClearFlags.Depth;
            secondaryCamera.clearFlags = CameraClearFlags.Depth;
            secondaryCamera.depth = 1; // rendered first
            Camera.main.depth = 2; // renders over secondary camera
 */
        }

        void Update()
        {
            if (!ReadyCheck())
                return;
            /*
            Debug.Log(string.Format("init: {0}, ready: {1}", streamingWorld.IsInit, streamingWorld.IsReady));

            if (!streamingWorld.IsInit || !streamingWorld.IsReady)
            {
                return;
            }
            */

            if (!dfUnity.MaterialReader.IsReady || dfUnity.MaterialReader.TextureReader.TextureFile == null)
                return;

            if (worldTerrainGameObject == null) // lazy creation
            {
                worldTerrainGameObject = generateWorldTerrain();

                // set up camera stack - AFTER layer "WorldTerrain" has been assigned to worldTerrainGameObject (is done in function generateWorldTerrain())
                Camera.main.clearFlags = CameraClearFlags.Depth;
                secondaryCamera.clearFlags = CameraClearFlags.Depth;
                secondaryCamera.depth = 1; // rendered first
                Camera.main.depth = 2; // renders over secondary camera
            }

            // Handle moving to new map pixel or first-time init
            DFPosition curMapPixel = playerGPS.CurrentMapPixel;
            if (curMapPixel.X != MapPixelX ||
                curMapPixel.Y != MapPixelY)
            {
                if (worldTerrainGameObject != null) // sometimes it can happen that this point is reached before worldTerrainGameObject was created, in such case we just skip
                {
                    worldTerrainGameObject.gameObject.transform.parent = streamingWorld.transform;

                    updatePositionWorldTerrain(ref worldTerrainGameObject);

                    MapPixelX = curMapPixel.X;
                    MapPixelY = curMapPixel.Y;
                }
            }
        }

        #endregion

        #region Private Methods

        private void updatePositionWorldTerrain(ref GameObject terrainGameObject)
        {
            // scale factor used for terrain heights to approximately match StreamingWorld's terrain heights
            float heightMapScaleFactor = TerrainHelper.baseHeightScale + TerrainHelper.noiseMapScale;
            
            int TerrainDistance = streamingWorld.TerrainDistance;

            // sinkHeight for world terrain height values inside TerrainDistance radius from player position
            float sinkHeight = (50.0f * streamingWorld.TerrainScale) / TerrainHelper.maxTerrainHeight;

            // reduce chance of geometry intersections of world terrain and detailed terrain from StreamingWorld component
            float extraTranslationY = -12.5f * streamingWorld.TerrainScale;

            // world scale computed as in StreamingWorld.cs and DaggerfallTerrain.cs scripts
            float scale = MapsFile.WorldMapTerrainDim * MeshReader.GlobalScale;

            // get displacement in world map pixels
            float xdif = +1 - playerGPS.CurrentMapPixel.X; // +1 needs to be added
            float ydif = worldMapHeight - 1 - playerGPS.CurrentMapPixel.Y;

            // create unity terrain object
            Terrain terrain = terrainGameObject.GetComponent<Terrain>();

            // global world level transform (for whole world map pixels)
            Vector3 globalTransform;
            globalTransform.x = xdif * scale;
            globalTransform.y = (heightMapScaleFactor * streamingWorld.TerrainScale / TerrainHelper.maxTerrainHeight) + extraTranslationY;
            globalTransform.z = -ydif * scale;

            // get camera position of camera tagged as MainCamera, this camera position is used for the correct computation (local) translation inside world map pixel of the world map terrain 
            GameObject mainCamera;
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            Vector3 cameraPos = mainCamera.transform.position;

            // used location [693,225] for debugging the local translation
            //Debug.Log(string.Format("camera pos x,z: {0} ,{1}", cameraPos.x, cameraPos.z));

            // local world level transform (for inter- world map pixels)
            float localTransformX = (float)Math.Floor(cameraPos.x / scale) * scale;
            float localTransformZ = (float)Math.Floor(cameraPos.z / scale) * scale;

            // Debug.Log(string.Format("localTransform x,z: {0} ,{1}", localTransformX, localTransformZ));

            // compute composite transform and apply it to terrain object
            Vector3 finalTransform = new Vector3(globalTransform.x + localTransformX, globalTransform.y, globalTransform.z + localTransformZ);
            terrainGameObject.gameObject.transform.localPosition = finalTransform;


            // restore (previously) decreased terrain height values in sink area
            float[,] heightValues = new float[TerrainDistance * 2, TerrainDistance * 2]; // only TerrainDistance * 2 height values are affected, not TerrainDistance * 2 + 1 values

            if ((backupSinkAreaCenterPosX != -1) && (backupSinkAreaCenterPosY != -1)) // no values needs to be restored on very first run (since nothing was decreased before...)
            {
                for (int y=-TerrainDistance; y<TerrainDistance; y++)
                {
                    for (int x=-TerrainDistance; x<TerrainDistance; x++)
                    {
                        int xpos = backupSinkAreaCenterPosX + x + 1;
                        int ypos = (worldMapHeight - 1 - backupSinkAreaCenterPosY) + y + 1;
                        if ((xpos >= 0) && (xpos < worldMapWidth) && (ypos >= 0) && (ypos < worldMapHeight))
                        {
                            heightValues[y + TerrainDistance, x + TerrainDistance] = worldHeights[ypos, xpos];
                        }
                    }
                }
                terrain.terrainData.SetHeights(backupSinkAreaCenterPosX - TerrainDistance + 1, worldMapHeight - 1 - backupSinkAreaCenterPosY - TerrainDistance + 1, heightValues);
            }

            backupSinkAreaCenterPosX = -1 + playerGPS.CurrentMapPixel.X;
            backupSinkAreaCenterPosY = -0 + playerGPS.CurrentMapPixel.Y;

            // decrease terrain height values in sink area
            for (int y = -TerrainDistance; y < TerrainDistance; y++)
            {
                for (int x = -TerrainDistance; x < TerrainDistance; x++)
                {
                    int xpos = backupSinkAreaCenterPosX + x + 1;
                    int ypos = (499 - backupSinkAreaCenterPosY) + y + 1;
                    if ((xpos >= 0) && (xpos < worldMapWidth) && (ypos >= 0) && (ypos < worldMapHeight))
                    {
                        heightValues[y + TerrainDistance, x + TerrainDistance] = worldHeights[ypos, xpos] - sinkHeight;
                    }
                }
            }
            terrain.terrainData.SetHeights(backupSinkAreaCenterPosX - TerrainDistance + 1, worldMapHeight - 1 - backupSinkAreaCenterPosY - TerrainDistance + 1, heightValues);

        }

        private GameObject generateWorldTerrain()
        {
            // scale factor used for terrain heights to approximately match StreamingWorld's terrain heights
            const float heightMapScaleFactor = TerrainHelper.baseHeightScale + TerrainHelper.noiseMapScale;
            
            // Create Unity Terrain game object
            GameObject terrainGameObject = Terrain.CreateTerrainGameObject(null);

            // assign terrainGameObject to layer "WorldTerrain" if available (used for rendering with secondary camera to prevent floating-point precision problems with huge clipping ranges)
            int layerExtendedTerrain = LayerMask.NameToLayer("WorldTerrain");
            if (layerExtendedTerrain != -1)
                terrainGameObject.layer = layerExtendedTerrain;

            MapPixelX = playerGPS.CurrentMapPixel.X;
            MapPixelY = playerGPS.CurrentMapPixel.Y;

            int worldMapResolution = Math.Max(worldMapWidth, worldMapHeight);

            if (worldHeights == null)
            {
                worldHeights = new float[worldMapResolution, worldMapResolution];
            }

            int[] climateMap = new int[worldMapResolution * worldMapResolution];
            for (int y = 0; y < worldMapHeight; y++)
            {
                for (int x = 0; x < worldMapWidth; x++)
                {
                    // get height data for this map pixel from world map and scale it to approximately match StreamingWorld's terrain heights
                    float sampleHeight = Convert.ToSingle(dfUnity.ContentReader.WoodsFileReader.GetHeightMapValue(x, y)) * heightMapScaleFactor;

                    // make ocean elevation the lower limit
                    if (sampleHeight < TerrainHelper.scaledOceanElevation)
                    {
                        sampleHeight = TerrainHelper.scaledOceanElevation;
                    }

                    // normalize with TerrainHelper.maxTerrainHeight
                    worldHeights[worldMapHeight - 1 - y, x] = Mathf.Clamp01(sampleHeight / TerrainHelper.maxTerrainHeight);
                            
                    // get climate record for this map pixel
                    int worldClimate = dfUnity.ContentReader.MapFileReader.GetClimateIndex(x, y);
                    climateMap[(worldMapHeight - 1 - y) * worldMapResolution + x] = worldClimate;
                }
            }

            // Basemap not used and is just pushed far away
            const float basemapDistance = 1000000f;

            // Ensure TerrainData is created
            Terrain terrain = terrainGameObject.GetComponent<Terrain>();
            if (terrain.terrainData == null)
            {
                // Setup terrain data
                TerrainData terrainData = new TerrainData();
                terrainData.name = "WorldTerrain geometry";

                // this is not really an assignment! you tell unity terrain what resolution you want for your heightmap and it will allocate resources and take the next power of 2 increased by 1 as heightmapResolution...
                terrainData.heightmapResolution = worldMapResolution;

                float heightmapResolution = terrainData.heightmapResolution;
                // Calculate width and length of terrain in world units
                float terrainSize = ((MapsFile.WorldMapTerrainDim * MeshReader.GlobalScale) * (heightmapResolution - 1.0f));


                terrainData.size = new Vector3(terrainSize, TerrainHelper.maxTerrainHeight, terrainSize);

                //terrainData.size = new Vector3(terrainSize, TerrainHelper.maxTerrainHeight * TerrainScale * worldMapResolution, terrainSize);
                terrainData.SetDetailResolution(worldMapResolution, 16);
                terrainData.alphamapResolution = worldMapResolution;
                terrainData.baseMapResolution = worldMapResolution;

                // Apply terrain data
                terrain.terrainData = terrainData;
                //(terrain.collider as TerrainCollider).terrainData = terrainData;
                terrain.basemapDistance = basemapDistance;
            }

            terrain.heightmapPixelError = 0; // 0 ... prevent unity terrain lod approach, set to higher values to enable it

            // Promote heights
            Vector3 size = terrain.terrainData.size;
            terrain.terrainData.size = new Vector3(size.x, TerrainHelper.maxTerrainHeight * streamingWorld.TerrainScale, size.z);
            terrain.terrainData.SetHeights(0, 0, worldHeights);


            // update world terrain position - do this before terrainGameObject.transform invocation, so that object2world matrix is updated with correct values
            updatePositionWorldTerrain(ref terrainGameObject);


            int tileMapDim = terrain.terrainData.heightmapResolution - 1;

            Color32[] tileMap = new Color32[tileMapDim * tileMapDim];

            // Assign tile data to tilemap
            Color32 tileColor = new Color32(0, 0, 0, 0);
            for (int y = 0; y < worldMapHeight; y++)
            {
                for (int x = 0; x < worldMapWidth; x++)
                {
                    // Get sample tile data
                    int climateIndex = climateMap[y * worldMapWidth + x];

                    // Assign to tileMap
                    tileColor.r = Convert.ToByte(climateIndex);
                    tileMap[y * tileMapDim + x] = tileColor;
                }
            }

            Texture2D tileMapTexture = new Texture2D(tileMapDim, tileMapDim, TextureFormat.RGB24, false);
            tileMapTexture.filterMode = FilterMode.Point;
            tileMapTexture.wrapMode = TextureWrapMode.Clamp;

            // Promote tileMap
            tileMapTexture.SetPixels32(tileMap);
            tileMapTexture.Apply(false);


            // currently only summer textures supported
            Texture2D textureAtlasDesertSummer = dfUnity.MaterialReader.TextureReader.GetTerrainTilesetTexture(2);
            textureAtlasDesertSummer.filterMode = FilterMode.Point;

            Texture2D textureAtlasWoodlandSummer = dfUnity.MaterialReader.TextureReader.GetTerrainTilesetTexture(302);
            textureAtlasWoodlandSummer.filterMode = FilterMode.Point;

            Texture2D textureAtlasMountainSummer = dfUnity.MaterialReader.TextureReader.GetTerrainTilesetTexture(102);
            textureAtlasMountainSummer.filterMode = FilterMode.Point;

            Texture2D textureAtlasSwampSummer = dfUnity.MaterialReader.TextureReader.GetTerrainTilesetTexture(402);
            textureAtlasSwampSummer.filterMode = FilterMode.Point;

            Material terrainMaterial = new Material(Shader.Find("Daggerfall/IncreasedTerrainTilemap"));
            terrainMaterial.name = string.Format("world terrain material");

            // Assign textures and parameters            
            terrainMaterial.SetTexture("_MainTex", tileMapTexture);
            terrainMaterial.SetTexture("_TileAtlasTexDesert", textureAtlasDesertSummer);
            terrainMaterial.SetTexture("_TileAtlasTexWoodland", textureAtlasWoodlandSummer);
            terrainMaterial.SetTexture("_TileAtlasTexMountain", textureAtlasMountainSummer);
            terrainMaterial.SetTexture("_TileAtlasTexSwamp", textureAtlasSwampSummer);
            terrainMaterial.SetTexture("_TilemapTex", tileMapTexture);
            terrainMaterial.SetInt("_TilemapDim", tileMapDim);

            terrainMaterial.mainTexture = tileMapTexture;

            terrainMaterial.SetFloat("_TerrainDistanceInWorldUnits", (streamingWorld.TerrainDistance + 1.5f) * MapsFile.WorldMapTerrainDim * MeshReader.GlobalScale); // 1.5f ... 1 extra terrain tile (in every direction) and half for the center terrain tile

            Vector3 vecWaterHeight = new Vector3(0.0f,TerrainHelper.scaledOceanElevation * streamingWorld.TerrainScale, 0.0f); // water height level on y-axis
            Vector3 vecWaterHeightTransformed = terrainGameObject.transform.TransformPoint(vecWaterHeight); // transform to world coordinates

            terrainMaterial.SetFloat("_WaterHeightTransformed", vecWaterHeightTransformed.y);

            // Promote material
            terrain.materialTemplate = terrainMaterial;

            return (terrainGameObject);
        }

        #endregion

        #region Startup/Shutdown Methods

        private bool ReadyCheck()
        {
            if (isReady)
                return true;

            if (dfUnity == null)
            {
                dfUnity = DaggerfallUnity.Instance;
            }

            // Do nothing if DaggerfallUnity not ready
            if (!dfUnity.IsReady)
            {
                DaggerfallUnity.LogMessage("ExtendedTerrainDistance: DaggerfallUnity component is not ready. Have you set your Arena2 path?");
                return false;
            }

            // Raise ready flag
            isReady = true;

            return true;
        }

        #endregion
    }
}