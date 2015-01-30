using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Implementation of Daggerfall's sky backgrounds.
    /// Works in both forward and deferred rendering paths (but cannot change between them at runtime).
    /// Forward path sets MainCamera to depth-only clear and draws sky directly into scene (uses custom texture clear for solid colour).
    /// Deferred path uses two cameras and OnPostRender in local camera for sky drawing (uses normal camera solid colour clear).
    /// Sets own camera depth to MainCamera.depth-1 so sky is drawn first in deferred path.
    /// 
    /// Multi-threaded. Use this for desktop builds or during development.
    /// 
    /// DO NOT ATTACH THIS SCRIPT TO MAINCAMERA GAMEOBJECT.
    /// Attach to an empty GameObject or use the prefab provided.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DaggerfallSkyThreaded : MonoBehaviour
    {
        #region Fields

        // Maximum timescale supported by SetByWorldTime()
        //public static float MaxTimeScale = 1000;

        public PlayerGPS LocalPlayerGPS;                                    // Set to local PlayerGPS
        [Range(0, 31)]
        public int SkyIndex = 16;                                           // Sky index for daytime skies
        [Range(0, 63)]
        public int SkyFrame = 31;                                           // Sky frame for daytime skies
        public bool IsNight = false;                                        // Swaps sky to night variant based on index
        public bool ShowStars = true;                                       // Draw stars onto night skies
        public Color SkyTintColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);      // Modulates output texture colour
        public float SkyColorScale = 1.0f;                                  // Scales sky color brighter or darker

        const int skyNativeWidth = 512;         // Native image width of sky image
        const int skyNativeHalfWidth = 256;     // Half native image width
        const int skyNativeHeight = 220;        // Native image height
        const float skyScale = 1.2f;            // Scale of sky image relative to display area
        const float skyHorizon = 0.30f;         // Higher the value lower the horizon

        DaggerfallUnity dfUnity;
        Camera mainCamera;
        Camera myCamera;
        Texture2D westTexture;
        Texture2D eastTexture;
        Texture2D clearTexture;
        Color cameraClearColor;
        Rect fullTextureRect = new Rect(0, 0, 1, 1);
        int lastSkyIndex = -1;
        int lastSkyFrame = -1;
        bool lastNightFlag = false;
        Rect westRect, eastRect;
        CameraClearFlags initialClearFlags;

        LoadSkyJob job;
        bool loadInProgress;

        #endregion

        void Start()
        {
            dfUnity = DaggerfallUnity.Instance;

            // Try to find local player GPS if not set
            if (LocalPlayerGPS == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player)
                {
                    LocalPlayerGPS = player.GetComponent<PlayerGPS>();
                }
            }

            // Find main camera gameobject
            GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
            if (go)
            {
                mainCamera = go.GetComponent<Camera>();
            }

            // Check main camera component
            if (!mainCamera)
            {
                DaggerfallUnity.LogMessage("DaggerfallSky could not find MainCamera object. Disabling sky.", true);
                gameObject.SetActive(false);
                return;
            }

            // Save starting clear flags
            initialClearFlags = mainCamera.clearFlags;

            // Get my camera
            myCamera = GetComponent<Camera>();
            if (!myCamera)
            {
                DaggerfallUnity.LogMessage("DaggerfallSky could not find local camera. Disabling sky.", true);
                gameObject.SetActive(false);
                return;
            }

            // My camera must not be on the same GameObject as MainCamera
            if (myCamera == mainCamera)
            {
                DaggerfallUnity.LogMessage("DaggerfallSky must not be attached to same GameObject as MainCamera. Disabling sky.", true);
                gameObject.SetActive(false);
                return;
            }

            // Setup cameras
            SetupCameras();
        }

        void OnEnable()
        {
            SetupCameras();
        }

        void OnDisable()
        {
            // Restore main camera clear flags so we left it how we found it
            if (mainCamera)
                mainCamera.clearFlags = initialClearFlags;
        }

        void Update()
        {
            // Do nothing if not ready
            if (!ReadyCheck())
                return;

            // Automate time of day updates
            if (dfUnity.Option_AutomateSky && LocalPlayerGPS)
                ApplyTimeAndSpace();

            // Update sky textures if frame changed
            if ((lastSkyFrame != SkyFrame || lastNightFlag != IsNight) && job != null)
            {
                if (job.SkyColorsOut != null)
                {
                    // Promote to texture if loaded
                    int targetFrame = SkyFrame;
                    bool flip = false;
                    if (SkyFrame >= 32)
                    {
                        targetFrame = 63 - SkyFrame;
                        flip = true;
                    }
                    if (!IsNight && job.SkyColorsOut[targetFrame].loaded)
                    {
                        PromoteToTexture(job.SkyColorsOut[targetFrame], flip);
                        lastSkyFrame = SkyFrame;
                        lastNightFlag = IsNight;
                    }
                    else if (IsNight && job.NightSkyColorsOut.loaded)
                    {
                        PromoteToTexture(job.NightSkyColorsOut);
                        lastSkyFrame = SkyFrame;
                        lastNightFlag = IsNight;
                    }
                }
            }

            // Check if job complete
            if (job != null && loadInProgress)
            {
                if (job.Update())
                {
                    loadInProgress = false;
                    lastSkyIndex = SkyIndex;
                }
            }

            // Load new sky set if index changed
            if (lastSkyIndex != SkyIndex && !loadInProgress)
            {
                StartThreadLoad();
            }

            // Forward paths are drawn here
            if (myCamera.renderingPath != RenderingPath.DeferredLighting)
            {
                UpdateSkyRects();
                DrawSky(false);
            }
        }

        void OnPostRender()
        {
            // Deferred path is drawn here
            if (myCamera.renderingPath == RenderingPath.DeferredLighting)
            {
                UpdateSkyRects();
                DrawSky(true);
            }
        }

        #region Private Methods

        private void UpdateSkyRects()
        {
            Vector3 angles = mainCamera.transform.eulerAngles;
            float width = Screen.width * skyScale;
            float height = Screen.height * skyScale;
            float halfScreenWidth = Screen.width * 0.5f;

            // Scroll left-right
            float percent = 0;
            float scrollX = 0;
            float westOffset = 0;
            float eastOffset = 0;
            if (angles.y >= 90f && angles.y < 180f)
            {
                percent = 1.0f - ((360f - angles.y) / 180f);
                scrollX = -width * percent;

                westOffset = -width + halfScreenWidth;
                eastOffset = halfScreenWidth;
            }
            else if (angles.y >= 0f && angles.y < 90f)
            {
                percent = 1.0f - ((360f - angles.y) / 180f);
                scrollX = -width * percent;

                westOffset = -width + halfScreenWidth;
                eastOffset = westOffset - width;
            }
            else if (angles.y >= 180f && angles.y < 270f)
            {
                percent = 1.0f - (angles.y / 180f);
                scrollX = width * percent;

                westOffset = -width + halfScreenWidth;
                eastOffset = halfScreenWidth;
            }
            else// if (angles.y >= 270f && angles.y < 360f)
            {
                percent = 1.0f - (angles.y / 180f);
                scrollX = width * percent;

                eastOffset = halfScreenWidth;
                westOffset = eastOffset + width;
            }

            // Scroll up-down
            float horizonY = -Screen.height + (Screen.height * skyHorizon);
            float scrollY = horizonY;
            if (angles.x >= 270f && angles.x < 360f)
            {
                // Scroll down until top of sky is aligned with top of screen
                percent = (360f - angles.x) / 75f;
                scrollY += height * percent;
                if (scrollY > 0) scrollY = 0;
            }
            else
            {
                // Keep scrolling up
                percent = angles.x / 75f;
                scrollY -= height * percent;
            }

            westRect = new Rect(westOffset + scrollX, scrollY, width, height);
            eastRect = new Rect(eastOffset + scrollX, scrollY, width, height);
        }

        private void DrawSky(bool deferred)
        {
            if (!westTexture || !eastTexture)
                return;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);

            // Clear display
            if (deferred)
            {
                // My camera just clears to solid colour in deferred mode
                // Reproduces tinting and scaling for clear colour
                myCamera.backgroundColor = ((cameraClearColor * SkyTintColor) * 2f) * SkyColorScale;
            }
            else
            {
                // Custom clear in forward mode using fullscreen texture
                Color finalColor = SkyTintColor * SkyColorScale;
                finalColor.a = 1f;
                Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
                Graphics.DrawTexture(screenRect, clearTexture, fullTextureRect, 0, 0, 0, 0, finalColor, null);
            }

            // Draw sky hemispheres
            Graphics.DrawTexture(westRect, westTexture, fullTextureRect, 0, 0, 0, 0, SkyTintColor * SkyColorScale, null);
            Graphics.DrawTexture(eastRect, eastTexture, fullTextureRect, 0, 0, 0, 0, SkyTintColor * SkyColorScale, null);

            GL.PopMatrix();
        }

        private void PromoteToTexture(LoadSkyJob.SkyColors colors, bool flip = false)
        {
            const int dayWidth = 512;
            const int dayHeight = 220;
            const int nightWidth = 512;
            const int nightHeight = 219;
            const int clearDim = 16;

            // Destroy old textures
            Destroy(westTexture);
            Destroy(eastTexture);

            // Create new textures
            if (!IsNight)
            {
                westTexture = new Texture2D(dayWidth, dayHeight, TextureFormat.RGB24, false);
                eastTexture = new Texture2D(dayWidth, dayHeight, TextureFormat.RGB24, false);
            }
            else
            {
                westTexture = new Texture2D(nightWidth, nightHeight, TextureFormat.RGB24, false);
                eastTexture = new Texture2D(nightWidth, nightHeight, TextureFormat.RGB24, false);
            }
            clearTexture = new Texture2D(clearDim, clearDim, TextureFormat.RGB24, false);

            // Set pixels, flipping hemisphere if required
            if (!flip)
            {
                westTexture.SetPixels32(colors.west);
                eastTexture.SetPixels32(colors.east);
            }
            else
            {
                westTexture.SetPixels32(colors.east);
                eastTexture.SetPixels32(colors.west);
            }

            // Set wrap mode
            eastTexture.wrapMode = TextureWrapMode.Clamp;
            westTexture.wrapMode = TextureWrapMode.Clamp;

            // Set filter mode
            westTexture.filterMode = dfUnity.MaterialReader.SkyFilterMode;
            eastTexture.filterMode = dfUnity.MaterialReader.SkyFilterMode;

            // Compress sky textures
            if (dfUnity.MaterialReader.CompressSkyTextures)
            {
                westTexture.Compress(true);
                eastTexture.Compress(true);
            }

            // Apply changes
            clearTexture.SetPixels32(colors.clear);
            westTexture.Apply(false, true);
            eastTexture.Apply(false, true);
            clearTexture.Apply(false, true);

            // Set camera clear colour
            cameraClearColor = colors.clearColor;

            // Assign colour to fog
            UnityEngine.RenderSettings.fogColor = cameraClearColor;
        }

        private void ApplyTimeAndSpace()
        {
            // Do nothing if timescale too fast or we'll be thrashing texture loads
            //if (dfUnity.WorldTime.TimeScale > MaxTimeScale)
            //    return;

            // Adjust sky index for climate and season
            // Season value enum ordered same as sky indices
            SkyIndex = LocalPlayerGPS.ClimateSettings.SkyBase + (int)dfUnity.WorldTime.SeasonValue;

            // Set night flag
            IsNight = dfUnity.WorldTime.IsNight;

            // Adjust sky frame by time of day
            if (!IsNight)
            {
                float minute = dfUnity.WorldTime.MinuteOfDay - WorldTime.DawnHour * WorldTime.MinutesPerHour;
                float divisor = ((WorldTime.DuskHour - WorldTime.DawnHour) * WorldTime.MinutesPerHour) / 64f;   // Total of 64 steps in daytime cycle
                float frame = minute / divisor;
                SkyFrame = (int)frame;
            }
        }

        private void StartThreadLoad()
        {
            // Do nothing if load already in progress
            if (loadInProgress)
                return;

            // Get night sky matching sky index
            int nightSky;
            if (SkyIndex >= 0 && SkyIndex <= 7)
                nightSky = 3;
            else if (SkyIndex >= 8 && SkyIndex <= 15)
                nightSky = 1;
            else if (SkyIndex >= 16 && SkyIndex <= 23)
                nightSky = 2;
            else
                nightSky = 0;

            // Load source binary data (this doesn't take long)
            string filename = string.Format("NITE{0:00}I0.IMG", nightSky);
            SkyFile skyFile = new SkyFile(Path.Combine(dfUnity.Arena2Path, SkyFile.IndexToFileName(SkyIndex)), FileUsage.UseMemory, true);
            ImgFile imgFile = new ImgFile(Path.Combine(dfUnity.Arena2Path, filename), FileUsage.UseMemory, true);
            imgFile.Palette.Load(Path.Combine(dfUnity.Arena2Path, imgFile.PaletteName));

            // Use threaded job to convert images (this is the expensive part)
            job = new LoadSkyJob();
            job.SkyFile = skyFile;
            job.ImgFile = imgFile;
            job.ShowStars = ShowStars;

            // Set priority
            if (IsNight)
            {
                job.PriorityNight = true;
                job.PriorityFrame = -1;
            }
            else
            {
                job.PriorityNight = false;
                if (SkyFrame >= 32)
                    job.PriorityFrame = 63 - SkyFrame;   
                else
                    job.PriorityFrame = SkyFrame;
            }

            // Start job
            job.Start();
            loadInProgress = true;
        }

        private bool ReadyCheck()
        {
            // Must have both world and sky cameras to draw
            if (!mainCamera || !myCamera)
                return false;

            // Do nothing if DaggerfallUnity not ready
            if (!dfUnity.IsReady)
            {
                DaggerfallUnity.LogMessage("DaggerfallSky: DaggerfallUnity component is not ready. Have you set your Arena2 path?");
                return false;
            }

            return true;
        }

        private void SetupCameras()
        {
            // Must have both cameras
            if (!mainCamera || !myCamera)
                return;

            myCamera.renderingPath = mainCamera.renderingPath;
            if (myCamera.renderingPath == RenderingPath.DeferredLighting)
            {
                myCamera.enabled = true;
                myCamera.depth = mainCamera.depth - 1;
                myCamera.cullingMask = 0;
                myCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.clearFlags = CameraClearFlags.Nothing;
            }
            else
            {
                myCamera.enabled = false;
                mainCamera.clearFlags = CameraClearFlags.Depth;
            }
        }

        #endregion

        #region Threaded Loading

        /// <summary>
        /// Pixel converting a sky from Daggerfall's palettized format to Color32[] array
        /// is an expensive operation which creates noticeable lag when frames ticks over.
        /// This class will thread-load sky images from binary when main class is
        /// instantiated or when SkyIndex changes and a new sky is required.
        /// All the main thread has to do is promote the cached Color32[] array to Material
        /// when loading a new frame.
        /// </summary>
        public class LoadSkyJob : ThreadedJob
        {
            public SkyFile SkyFile;
            public ImgFile ImgFile;
            public int PriorityFrame = -1;
            public bool PriorityNight = false;
            public bool ShowStars = true;

            public SkyColors[] SkyColorsOut;
            public SkyColors NightSkyColorsOut;

            System.Random random = new System.Random();
            float starChance = 0.004f;
            byte[] starColorIndices = new byte[] { 16, 32, 74, 105, 112, 120 };     // Some random sky colour indices

            //long totalTime;

            public struct SkyColors
            {
                public Color32[] west;
                public Color32[] east;
                public Color32[] clear;
                public Color clearColor;
                public bool loaded;
            }

            protected override void ThreadFunction()
            {
                //System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                //long startTime = stopwatch.ElapsedMilliseconds;

                const int frameCount = 32;

                // Create outgoing data
                SkyColorsOut = new SkyColors[frameCount];
                NightSkyColorsOut = new SkyColors();

                // Load priority sky
                if (PriorityNight)
                    LoadSingleNightSky();
                else if (PriorityFrame >= 0)
                    LoadSingleSky(PriorityFrame);

                // Read day sky images
                for (int frame = 0; frame < frameCount; frame++)
                {
                    LoadSingleSky(frame);
                }

                // Read night sky image
                LoadSingleNightSky();

                //totalTime = stopwatch.ElapsedMilliseconds - startTime;
            }

            protected override void OnFinished()
            {
                //Debug.Log("Total time to load sky images: " + totalTime.ToString());
            }

            private void LoadSingleSky(int frame)
            {
                if (SkyColorsOut[frame].loaded)
                    return;

                SkyFile.Palette = SkyFile.GetDFPalette(frame);
                SkyColorsOut[frame].east = SkyFile.GetColors32(0, frame);
                SkyColorsOut[frame].west = SkyFile.GetColors32(1, frame);
                SkyColorsOut[frame].clearColor = SkyColorsOut[frame].west[0];
                SkyColorsOut[frame].clear = CreateClearColors(SkyColorsOut[frame].clearColor);
                SkyColorsOut[frame].loaded = true;
            }

            private void LoadSingleNightSky()
            {
                const int width = 512;
                const int height = 219;

                if (NightSkyColorsOut.loaded)
                    return;

                // Get sky bitmap
                DFBitmap dfBitmap = ImgFile.GetDFBitmap(0, 0);

                // Draw stars
                if (ShowStars)
                {
                    for (int i = 0; i < dfBitmap.Data.Length; i++)
                    {
                        // Stars should only be drawn over clear sky indices
                        int index = dfBitmap.Data[i];
                        if (index > 16 && index < 32)
                        {
                            if (random.NextDouble() < starChance)
                                dfBitmap.Data[i] = starColorIndices[random.Next(0, starColorIndices.Length)];
                        }
                    }
                }

                // Get sky colour array
                Color32[] colors = ImgFile.GetColors32(ref dfBitmap);

                // Fix seam on right side of night skies
                for (int y = 0; y < height; y++)
                {
                    int pos = y * width + width - 2;
                    colors[pos + 1] = colors[pos];
                }

                NightSkyColorsOut.west = colors;
                NightSkyColorsOut.east = colors;
                NightSkyColorsOut.clearColor = NightSkyColorsOut.west[0];
                NightSkyColorsOut.clear = CreateClearColors(NightSkyColorsOut.clearColor);

                NightSkyColorsOut.loaded = true;
            }

            private Color32[] CreateClearColors(Color clearColor)
            {
                const int dim = 16;

                // Create clear colour array for small clear texture
                // Used to clear sky background in forward rendering
                Color32[] clearColors = new Color32[dim * dim];
                for (int i = 0; i < clearColors.Length; i++)
                {
                    clearColors[i] = clearColor;
                }

                return clearColors;
            }
        }

        #endregion
    }
}
