using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Helper for laying out RMB (city block) data in scene.
    /// Vertices can be welded based on Option_CombineRMB in DaggerfallUnity singleton.
    /// </summary>
    public class RMBLayout
    {
        public static float RMBSide = 4096f * MeshReader.GlobalScale;

        const uint cityGateOpenId = 446;
        const uint cityGateClosedId = 447;

        DaggerfallUnity dfUnity;
        DFBlock blockData;
        //DaggerfallBlock dfBlock;

        ModelCombiner combiner = new ModelCombiner();

        [NonSerialized]
        public static float PropsOffsetY = -6f;
        [NonSerialized]
        public static float BlockFlatsOffsetY = -6f;
        [NonSerialized]
        public static float NatureFlatsOffsetY = -2f;

        public RMBLayout(DaggerfallUnity dfUnity, string blockName)
        {
            blockData = dfUnity.ContentReader.BlockFileReader.GetBlock(blockName);
            if (blockData.Type != DFBlock.BlockTypes.Rmb)
                throw new Exception(string.Format("Could not load RMB block {0}", blockName), null);

            this.dfUnity = dfUnity;
        }

        /// <summary>
        /// Creates a new RMB GameObject and performs block layout.
        /// </summary>
        /// <param name="dfUnity">DaggerfallUnity singleton. Required for content readers and settings.</param>
        /// <param name="blockName">Name of RMB block to build.</param>
        /// <param name="climateBase">Climate base for texture swaps.</param>
        /// <param name="natureSet">Nature set for texture swaps.</param>
        /// <param name="climateSeason">Season modifier for texture swaps.</param>
        /// <returns>GameObject.</returns>
        public static GameObject CreateGameObject(
            DaggerfallUnity dfUnity,
            string blockName,
            ClimateBases climateBase = ClimateBases.Temperate,
            ClimateNatureSets natureSet = ClimateNatureSets.TemperateWoodland,
            ClimateSeason climateSeason = ClimateSeason.Summer)
        {
            // Validate
            if (string.IsNullOrEmpty(blockName))
                return null;
            if (!blockName.ToUpper().EndsWith(".RMB"))
                return null;

            // Start layout
            RMBLayout layout = new RMBLayout(dfUnity, blockName);

            // Create gameobject
            GameObject go = new GameObject(string.Format("DaggerfallBlock [Name={0}]", blockName));
            go.AddComponent<DaggerfallBlock>();

            // Layout exterior
            layout.AddModels(go.transform, climateBase, climateSeason);
            layout.AddProps(go.transform, climateBase, climateSeason);
            layout.AddBlockFlats(go.transform);
            layout.AddNatureFlats(go.transform, natureSet, climateSeason);
            layout.AddGroundPlane(go.transform, climateBase, climateSeason);

            return go;
        }

        #region Private Methods

        /// <summary>
        /// Add RMB models to parent transform.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="climateBase">Climate base for texture swaps.</param>
        /// <param name="climateSeason">Season modifier for texture swaps.</param>
        private void AddModels(
            Transform parent,
            ClimateBases climateBase = ClimateBases.Temperate,
            ClimateSeason climateSeason = ClimateSeason.Summer)
        {
            GameObject node = new GameObject("Models");
            if (parent != null)
                node.transform.parent = parent;

            // This list receives all static doors for single or combined mesh
            List<DaggerfallStaticDoors.StaticDoor> allStaticDoors = new List<DaggerfallStaticDoors.StaticDoor>();

            // Iterate through all subrecords
            int recordCount = 0;
            combiner.NewCombiner();
            foreach (DFBlock.RmbSubRecord subRecord in blockData.RmbBlock.SubRecords)
            {
                // Get subrecord transform
                Vector3 subRecordPosition = new Vector3(subRecord.XPos, 0, BlocksFile.RMBDimension - subRecord.ZPos) * MeshReader.GlobalScale;
                Vector3 subRecordRotation = new Vector3(0, -subRecord.YRotation / BlocksFile.RotationDivisor, 0);
                Matrix4x4 subRecordMatrix = Matrix4x4.TRS(subRecordPosition, Quaternion.Euler(subRecordRotation), Vector3.one);

                // Iterate through models in this subrecord
                foreach (DFBlock.RmbBlock3dObjectRecord obj in subRecord.Exterior.Block3dObjectRecords)
                {
                    // Get model transform
                    Vector3 modelPosition = new Vector3(obj.XPos, -obj.YPos, obj.ZPos) * MeshReader.GlobalScale;
                    Vector3 modelRotation = new Vector3(0, -obj.YRotation / BlocksFile.RotationDivisor, 0);
                    Matrix4x4 modelMatrix = subRecordMatrix * Matrix4x4.TRS(modelPosition, Quaternion.Euler(modelRotation), Vector3.one);

                    // Override combine and city gate flags
                    bool overrideCombine = false;
                    bool isCityGate = false;

                    // Get model ID
                    ModelData modelData;
                    uint modelId = obj.ModelIdNum;

                    // City gates are never combined as this can change at runtime
                    if (modelId == cityGateOpenId || modelId == cityGateClosedId)
                    {
                        overrideCombine = true;
                        isCityGate = true;
                    }

                    // City gates open or closed?
                    if (modelId == cityGateOpenId && dfUnity.Option_CloseCityGates)
                        modelId = cityGateClosedId;
                    else if (modelId == cityGateClosedId && !dfUnity.Option_CloseCityGates)
                        modelId = cityGateOpenId;

                    // Get model data
                    dfUnity.MeshReader.GetModelData(modelId, out modelData);

                    // Get array of static doors from model data
                    allStaticDoors.AddRange(GameObjectHelper.GetStaticDoors(ref modelData, blockData.Index, recordCount, modelMatrix));

                    // Combine or add
                    if (dfUnity.Option_CombineRMB && !overrideCombine)
                    {
                        combiner.Add(ref modelData, modelMatrix);
                    }
                    else
                    {
                        // Add GameObject
                        GameObject go = GameObjectHelper.CreateDaggerfallMeshGameObject(dfUnity, modelId, node.transform, dfUnity.Option_SetStaticFlags);
                        go.transform.position = modelMatrix.GetColumn(3);
                        go.transform.rotation = GameObjectHelper.QuaternionFromMatrix(modelMatrix);

                        // Add static doors component
                        DaggerfallStaticDoors c = go.AddComponent<DaggerfallStaticDoors>();
                        c.Doors = allStaticDoors.ToArray();

                        // Update climate
                        DaggerfallMesh dfMesh = go.GetComponent<DaggerfallMesh>();
                        dfMesh.SetClimate(dfUnity, climateBase, climateSeason, WindowStyle.Disabled);

                        // Add city gate component
                        if (isCityGate)
                        {
                            DaggerfallCityGate gate = go.AddComponent<DaggerfallCityGate>();
                            gate.SetOpen(!dfUnity.Option_CloseCityGates);
                        }
                    }
                }

                // Increment record count
                recordCount++;
            }

            // Add combined GameObject
            if (dfUnity.Option_CombineRMB)
            {
                if (combiner.VertexCount > 0)
                {
                    // Add combined models
                    combiner.Apply();
                    GameObject go = GameObjectHelper.CreateCombinedMeshGameObject(dfUnity, combiner, "CombinedModels", node.transform, dfUnity.Option_SetStaticFlags);

                    // Add static doors component
                    DaggerfallStaticDoors c = go.AddComponent<DaggerfallStaticDoors>();
                    c.Doors = allStaticDoors.ToArray();

                    // Update climate
                    DaggerfallMesh dfMesh = go.GetComponent<DaggerfallMesh>();
                    dfMesh.SetClimate(dfUnity, climateBase, climateSeason, WindowStyle.Disabled);
                }
            }
        }

        /// <summary>
        /// Add RMB props to parent transform.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="climateBase">Climate base for texture swaps.</param>
        /// <param name="climateSeason">Season modifier for texture swaps.</param>
        private void AddProps(
            Transform parent,
            ClimateBases climateBase = ClimateBases.Temperate,
            ClimateSeason climateSeason = ClimateSeason.Summer)
        {
            GameObject node = new GameObject("Props");
            node.transform.parent = parent;

            // Iterate through all misc records
            combiner.NewCombiner();
            foreach (DFBlock.RmbBlock3dObjectRecord obj in blockData.RmbBlock.Misc3dObjectRecords)
            {
                // Get model transform
                Vector3 modelPosition = new Vector3(obj.XPos, -obj.YPos + PropsOffsetY, obj.ZPos + BlocksFile.RMBDimension) * MeshReader.GlobalScale;
                Vector3 modelRotation = new Vector3(0, -obj.YRotation / BlocksFile.RotationDivisor, 0);
                Matrix4x4 modelMatrix = Matrix4x4.TRS(modelPosition, Quaternion.Euler(modelRotation), Vector3.one);

                // Combine or add
                if (dfUnity.Option_CombineRMB)
                {
                    // Combine model data
                    ModelData modelData;
                    dfUnity.MeshReader.GetModelData(obj.ModelIdNum, out modelData);
                    combiner.Add(ref modelData, modelMatrix);
                }
                else
                {
                    // Add GameObject
                    GameObject go = GameObjectHelper.CreateDaggerfallMeshGameObject(dfUnity, obj.ModelIdNum, node.transform, dfUnity.Option_SetStaticFlags);
                    go.transform.position = modelMatrix.GetColumn(3);
                    go.transform.rotation = GameObjectHelper.QuaternionFromMatrix(modelMatrix);

                    // Update climate
                    DaggerfallMesh dfMesh = go.GetComponent<DaggerfallMesh>();
                    dfMesh.SetClimate(dfUnity, climateBase, climateSeason, WindowStyle.Disabled);
                }
            }

            // Add combined GameObject
            if (dfUnity.Option_CombineRMB)
            {
                if (combiner.VertexCount > 0)
                {
                    combiner.Apply();
                    GameObject go = GameObjectHelper.CreateCombinedMeshGameObject(dfUnity, combiner, "CombinedProps", node.transform, dfUnity.Option_SetStaticFlags);

                    // Update climate
                    DaggerfallMesh dfMesh = go.GetComponent<DaggerfallMesh>();
                    dfMesh.SetClimate(dfUnity, climateBase, climateSeason, WindowStyle.Disabled);
                }
            }
        }

        /// <summary>
        /// Add RMB flats to parent transform.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        private void AddBlockFlats(Transform parent)
        {
            GameObject node = new GameObject("Block Flats");
            node.transform.parent = parent;

            // Add block flats
            foreach (DFBlock.RmbBlockFlatObjectRecord obj in blockData.RmbBlock.MiscFlatObjectRecords)
            {
                // Spawn billboard gameobject
                GameObject go = GameObjectHelper.CreateDaggerfallBillboardGameObject(dfUnity, obj.TextureArchive, obj.TextureRecord, node.transform);
                go.transform.position = new Vector3(obj.XPos, -obj.YPos + BlockFlatsOffsetY, obj.ZPos + BlocksFile.RMBDimension) * MeshReader.GlobalScale;

                // Add lights
                if (obj.TextureArchive == 210 && dfUnity.Option_ImportPointLights)
                {
                    // Spawn light gameobject
                    Vector2 size = dfUnity.MeshReader.GetScaledBillboardSize(210, obj.TextureRecord);
                    GameObject lightgo = GameObjectHelper.CreateDaggerfallRMBPointLight(dfUnity, go.transform);
                    lightgo.transform.position = new Vector3(obj.XPos, -obj.YPos + size.y, obj.ZPos + BlocksFile.RMBDimension) * MeshReader.GlobalScale;

                    // Animate light
                    DaggerfallLight c = lightgo.AddComponent<DaggerfallLight>();
                    c.ParentBillboard = go.GetComponent<DaggerfallBillboard>();
                    if (dfUnity.Option_AnimatedPointLights)
                    {
                        c.Animate = true;
                    }
                }
            }
        }

        /// <summary>
        /// Add RMB nature flats to parent transform.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="archive">Archive index for nature flats.</param>
        /// <param name="climateBase">Climate base for texture swaps.</param>
        /// <param name="climateSeason">Season modifier for texture swaps.</param>
        private void AddNatureFlats(
            Transform parent,
            ClimateNatureSets natureSet = ClimateNatureSets.TemperateWoodland,
            ClimateSeason climateSeason = ClimateSeason.Summer)
        {
            int archive = ClimateSwaps.GetNatureArchive(natureSet, climateSeason);

            GameObject node = new GameObject("Nature Flats");
            node.transform.parent = parent;

            // Add block scenery
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // Get scenery item
                    DFBlock.RmbGroundScenery scenery = blockData.RmbBlock.FldHeader.GroundData.GroundScenery[x, 15 - y];

                    // Ignore 0 as this appears to be a marker/waypoint of some kind
                    if (scenery.TextureRecord > 0)
                    {
                        // Spawn billboard gameobject
                        GameObject go = GameObjectHelper.CreateDaggerfallBillboardGameObject(dfUnity, archive, scenery.TextureRecord, node.transform);
                        Vector3 billboardPosition = new Vector3(
                            x * BlocksFile.TileDimension, 
                            NatureFlatsOffsetY,
                            y * BlocksFile.TileDimension + BlocksFile.TileDimension) * MeshReader.GlobalScale;

                        // Set transform
                        go.transform.position = billboardPosition;
                    }
                }
            }
        }

        /// <summary>
        /// Add RMB ground plane to parent transform.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="climateBase">Climate base for texture swaps.</param>
        /// <param name="climateSeason">Season modifier for texture swaps.</param>
        private void AddGroundPlane(
            Transform parent,
            ClimateBases climateBase = ClimateBases.Temperate,
            ClimateSeason climateSeason = ClimateSeason.Summer)
        {
            // Create gameobject
            GameObject go = new GameObject("GroundPlane");
            if (parent) go.transform.parent = parent;

            // Assign components
            DaggerfallGroundMesh dfGround = go.AddComponent<DaggerfallGroundMesh>();
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();

            // Assign climate and mesh
            dfGround.SetClimate(dfUnity, climateBase, climateSeason);
            Mesh mesh = dfUnity.MeshReader.GetGroundMesh(
                ref blockData,
                dfGround.Summary.rects,
                dfUnity.MeshReader.AddMeshTangents,
                dfUnity.MeshReader.AddMeshLightmapUVs);
            if (mesh)
            {
                meshFilter.sharedMesh = mesh;
            }

            // Assign collider
            if (dfUnity.Option_AddMeshColliders)
                go.AddComponent<BoxCollider>();

            // Assign static
            if (dfUnity.Option_SetStaticFlags)
                go.isStatic = true;
        }

        #endregion
    }
}