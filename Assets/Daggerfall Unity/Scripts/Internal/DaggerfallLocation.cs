using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    public class DaggerfallLocation : MonoBehaviour
    {
        bool isSet = false;
        DaggerfallUnity dfUnity;

        [SerializeField]
        private LocationSummary summary;

        // Climate texture swaps
        public LocationClimateUse ClimateUse = LocationClimateUse.UseLocation;
        public ClimateBases CurrentClimate = ClimateBases.Temperate;
        public ClimateSeason CurrentSeason = ClimateSeason.Summer;
        public ClimateNatureSets CurrentNatureSet = ClimateNatureSets.TemperateWoodland;

        // Window texture swaps
        public WindowStyle WindowTextureStyle = WindowStyle.Disabled;

        // Dungeon texture swaps
        public DungeonTextureUse DungeonTextureUse = DungeonTextureUse.Disabled;
        public int[] DungeonTextureTable = new int[] { 119, 120, 122, 123, 124, 168 };

        // Internal time and space texture swaps
        int lastClimate = -1;
        WorldTime.Seasons lastSeason;
        bool lastCityLightsFlag;

        public LocationSummary Summary
        {
            get { return summary; }
        }

        [Serializable]
        public struct LocationSummary
        {
            public int ID;
            public int Longitude;
            public int Latitude;
            public int MapPixelX;
            public int MapPixelY;
            public int WorldCoordX;
            public int WorldCoordZ;
            public string RegionName;
            public string LocationName;
            public int WorldClimate;
            public DFRegion.LocationTypes LocationType;
            public DFRegion.DungeonTypes DungeonType;
            public bool HasDungeon;
            public bool InDungeon;
            public ClimateBases Climate;
            public ClimateNatureSets Nature;
            public int SkyBase;
        }

        public void Update()
        {
            // Do nothing if not ready
            if (!ReadyCheck())
                return;

            // Handle automated texture swaps
            if (dfUnity.Option_AutomateTextureSwaps)
            {
                // Only process if climate, season, day/night, or weather changed
                if (lastClimate != dfUnity.PlayerGPS.CurrentClimate ||
                    lastSeason != dfUnity.WorldTime.SeasonValue ||
                    lastCityLightsFlag != dfUnity.WorldTime.CityLightsOn)
                {
                    ApplyTimeAndSpace();
                    lastClimate = dfUnity.PlayerGPS.CurrentClimate;
                    lastSeason = dfUnity.WorldTime.SeasonValue;
                    lastCityLightsFlag = dfUnity.WorldTime.CityLightsOn;
                }
            }
        }

        private void ApplyTimeAndSpace()
        {
            // No effect in dungeons
            if (summary.InDungeon)
                return;

            // TODO: Handle setting appropriate textures for weather

            // Get season and weather
            if (dfUnity.WorldTime.SeasonValue == WorldTime.Seasons.Winter)
                CurrentSeason = ClimateSeason.Winter;
            else
                CurrentSeason = ClimateSeason.Summer;

            // Set windows
            if (dfUnity.WorldTime.CityLightsOn)
                WindowTextureStyle = WindowStyle.Night;
            else
                WindowTextureStyle = WindowStyle.Day;

            // Apply changes
            ApplyClimateSettings();
        }

        public void SetLocation(DFLocation location, bool dungeon)
        {
            // Validate
            if (this.isSet)
                throw new Exception("This location has already been set.");
            if (!location.Loaded)
                throw new Exception("DFLocation not loaded.");
            if (dungeon && !location.HasDungeon)
                throw new Exception("DFLocation does not contain a dungeon.");

            // Find DaggerfallUnity
            if (!DaggerfallUnity.FindDaggerfallUnity(out dfUnity))
                return;

            // Set summary
            summary = new LocationSummary();
            summary.ID = location.MapTableData.MapId;
            summary.Longitude = (int)location.MapTableData.Longitude;
            summary.Latitude = (int)location.MapTableData.Latitude;
            DFPosition mapPixel = MapsFile.GetMapPixel(summary.Longitude, summary.Latitude);
            DFPosition worldCoord = MapsFile.MapPixelToWorldCoord(mapPixel.X, mapPixel.Y);
            summary.MapPixelX = mapPixel.X;
            summary.MapPixelY = mapPixel.Y;
            summary.WorldCoordX = worldCoord.X;
            summary.WorldCoordZ = worldCoord.Y;
            summary.RegionName = location.RegionName;
            summary.LocationName = location.Name;
            summary.WorldClimate = location.Climate.WorldClimate;
            summary.LocationType = location.MapTableData.Type;
            summary.DungeonType = location.MapTableData.DungeonType;
            summary.HasDungeon = location.HasDungeon;
            summary.InDungeon = dungeon;
            summary.Climate = ClimateSwaps.FromAPIClimateBase(location.Climate.ClimateType);
            summary.Nature = ClimateSwaps.FromAPITextureSet(location.Climate.NatureSet);
            summary.SkyBase = location.Climate.SkyBase;

            // Assign starting climate
            CurrentSeason = ClimateSeason.Summer;
            CurrentClimate = summary.Climate;
            CurrentNatureSet = summary.Nature;

            // Perform layout
            if (!dungeon)
                LayoutCity(ref location);
            else
                LayoutDungeon(ref location);

            // Seal location
            isSet = true;
        }

        public void ApplyClimateSettings()
        {
            // Do nothing if not ready
            if (!ReadyCheck())
                return;

            // Process all DaggerfallMesh child components
            DaggerfallMesh[] meshArray = GetComponentsInChildren<DaggerfallMesh>();
            foreach (var dm in meshArray)
            {
                switch (ClimateUse)
                {
                    case LocationClimateUse.UseLocation:
                        dm.SetClimate(dfUnity, Summary.Climate, CurrentSeason, WindowTextureStyle);
                        break;
                    case LocationClimateUse.Custom:
                        dm.SetClimate(dfUnity, CurrentClimate, CurrentSeason, WindowTextureStyle);
                        break;
                    case LocationClimateUse.Disabled:
                        dm.DisableClimate(dfUnity);
                        break;
                }
            }

            // Process all DaggerfallGroundMesh child components
            DaggerfallGroundMesh[] groundMeshArray = GetComponentsInChildren<DaggerfallGroundMesh>();
            foreach (var gm in groundMeshArray)
            {
                switch (ClimateUse)
                {
                    case LocationClimateUse.UseLocation:
                        gm.SetClimate(dfUnity, Summary.Climate, CurrentSeason);
                        break;
                    case LocationClimateUse.Custom:
                        gm.SetClimate(dfUnity, CurrentClimate, CurrentSeason);
                        break;
                    case LocationClimateUse.Disabled:
                        gm.SetClimate(dfUnity, ClimateBases.Temperate, ClimateSeason.Summer);
                        break;
                }
            }

            // Determine correct nature archive
            int natureArchive;
            switch (ClimateUse)
            {
                case LocationClimateUse.UseLocation:
                    natureArchive = ClimateSwaps.GetNatureArchive(summary.Nature, CurrentSeason);
                    break;
                case LocationClimateUse.Custom:
                    natureArchive = ClimateSwaps.GetNatureArchive(CurrentNatureSet, CurrentSeason);
                    break;
                case LocationClimateUse.Disabled:
                    default:
                    natureArchive = ClimateSwaps.GetNatureArchive(ClimateNatureSets.TemperateWoodland, ClimateSeason.Summer);
                    break;
            }

            // Process all DaggerfallBillboard child components
            DaggerfallBillboard[] billboardArray = GetComponentsInChildren<DaggerfallBillboard>();
            foreach (var db in billboardArray)
            {
                if (db.Summary.FlatType == FlatTypes.Nature)
                {
                    // Apply recalculated nature archive
                    db.SetMaterial(dfUnity, natureArchive, db.Summary.Record, 0, db.Summary.InDungeon);
                }
                else
                {
                    // All other flats are just reapplied to handle any other changes
                    db.SetMaterial(dfUnity, db.Summary.Archive, db.Summary.Record, 0, db.Summary.InDungeon);
                }
            }
        }

        public void RandomiseDungeonTextureTable()
        {
            // Valid dungeon textures table indices
            int[] valids = new int[]
            {
                019, 020, 022, 023, 024, 068,
                119, 120, 122, 123, 124, 168,
                319, 320, 322, 323, 324, 368,
                419, 420, 422, 423, 424, 468,
            };

            // Repopulate table
            for (int i = 0; i < DungeonTextureTable.Length; i++)
            {
                DungeonTextureTable[i] = valids[UnityEngine.Random.Range(0, valids.Length)];
            }

            ApplyDungeonTextureTable();
        }

        public void ResetDungeonTextureTable()
        {
            DungeonTextureTable[0] = 119;
            DungeonTextureTable[1] = 120;
            DungeonTextureTable[2] = 122;
            DungeonTextureTable[3] = 123;
            DungeonTextureTable[4] = 124;
            DungeonTextureTable[5] = 168;
            ApplyDungeonTextureTable();
        }

        public void ApplyDungeonTextureTable()
        {
            // Do nothing if not ready
            if (!ReadyCheck())
                return;

            // Process all DaggerfallMesh child components
            DaggerfallMesh[] meshArray = GetComponentsInChildren<DaggerfallMesh>();
            foreach (var dm in meshArray)
            {
                dm.SetDungeonTextures(dfUnity, DungeonTextureTable);
            }
        }

        #region Private Layout Methods

        private void LayoutCity(ref DFLocation location)
        {
            // Get city dimensions
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;
            
#if UNITY_EDITOR
            // Start timing
            Stopwatch stopwatch = Stopwatch.StartNew();
            long startTime = stopwatch.ElapsedMilliseconds;
            int total = width * height;
            int count = 0;
#endif

            // Import city blocks
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    string blockName = dfUnity.ContentReader.BlockFileReader.CheckName(dfUnity.ContentReader.MapFileReader.GetRmbBlockName(ref location, x, y));
                    GameObject go = RMBLayout.CreateGameObject(
                        dfUnity,
                        blockName,
                        Summary.Climate,
                        Summary.Nature,
                        CurrentSeason);
                    go.transform.parent = this.transform;
                    go.transform.position = new Vector3((x * RMBLayout.RMBSide), 0, (y * RMBLayout.RMBSide));

#if UNITY_EDITOR
                    // Update status
                    string status = string.Format("Importing RMB block {0}/{1}", count, total);
                    EditorUtility.DisplayProgressBar("Importing City", status, (float)count / (float)total);
                    count++;
#endif
                }
            }

#if UNITY_EDITOR
            // Show timer and clear status
            long totalTime = stopwatch.ElapsedMilliseconds - startTime;
            DaggerfallUnity.LogMessage(string.Format("Time to layout city: {0}ms", totalTime), true);
            EditorUtility.ClearProgressBar();
#endif
        }

        private void LayoutDungeon(ref DFLocation location)
        {
            // Start timing
            Stopwatch stopwatch = Stopwatch.StartNew();
            long startTime = stopwatch.ElapsedMilliseconds;

            // Create dungeon layout
            foreach (var block in location.Dungeon.Blocks)
            {
                GameObject go = RDBLayout.CreateGameObject(dfUnity, block.BlockName);
                go.transform.parent = this.transform;
                go.transform.position = new Vector3(block.X * RDBLayout.RDBSide, 0, block.Z * RDBLayout.RDBSide);
            }

            // Show timer
            long totalTime = stopwatch.ElapsedMilliseconds - startTime;
            DaggerfallUnity.LogMessage(string.Format("Time to layout dungeon: {0}ms", totalTime), true);
        }

        private bool ReadyCheck()
        {
            // Ensure we have a DaggerfallUnity reference
            if (dfUnity == null)
            {
                if (!DaggerfallUnity.FindDaggerfallUnity(out dfUnity))
                {
                    DaggerfallUnity.LogMessage("DaggerfallLocation: Could not get DaggerfallUnity component.");
                    return false;
                }
            }

            // Do nothing if DaggerfallUnity not ready
            if (!dfUnity.IsReady)
            {
                DaggerfallUnity.LogMessage("DaggerfallLocation: DaggerfallUnity component is not ready. Have you set your Arena2 path?");
                return false;
            }

            return true;
        }

        #endregion
    }
}
