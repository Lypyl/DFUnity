using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Tracks player position in world space.
    /// Not fully implemented.
    /// </summary>
    public class PlayerGPS : MonoBehaviour
    {
        public int WorldX;      // Player X coordinate in Daggerfall world units (0-32768000)
        public int WorldZ;      // Player Z coordinate in Daggerfall world units (0-16384000)

        DaggerfallUnity dfUnity;
        int mapPixelX = -1;
        int mapPixelY = -1;
        int currentClimate;
        int currentPolitic;
        DFLocation.ClimateSettings climateSettings;

        private Dictionary<int, MapSummary> mapDict;
        public struct MapSummary
        {
            public int ID;
            public int RegionIndex;
            public int MapIndex;
            public DFRegion.RegionMapTable MapTable;
        }

        /// <summary>
        /// Gets current player map pixel.
        /// </summary>
        public DFPosition CurrentMapPixel
        {
            get { return new DFPosition(mapPixelX, mapPixelY); }
        }

        /// <summary>
        /// Gets climate index based on player world position.
        /// </summary>
        public int CurrentClimate
        {
            get { return currentClimate; }
        }

        public int CurrentPolitic
        {
            get { return currentPolitic; }
        }

        /// <summary>
        /// Gets climate properties based on player world position.
        /// </summary>
        public DFLocation.ClimateSettings ClimateSettings
        {
            get { return climateSettings; }
        }

        void Start()
        {
        }

        void Update()
        {
            // Do nothing if not ready
            if (!ReadyCheck())
                return;

            // Update climate whenever player map pixel changes
            DFPosition pos = MapsFile.WorldCoordToMapPixel(WorldX, WorldZ);
            if (pos.X != mapPixelX || pos.Y != mapPixelY)
            {
                UpdateClimate(pos.X, pos.Y);
                mapPixelX = pos.X;
                mapPixelY = pos.Y;
            }
        }

        #region Public Methods

        /// <summary>
        /// Determines if the current WorldCoord has a location.
        /// </summary>
        /// <param name="x">Map pixel X.</param>
        /// <param name="y">Map pixel Y.</param>
        /// <returns>True if there is a location at this map pixel.</returns>
        public bool HasLocation(int x, int y, out MapSummary summaryOut)
        {
            int id = MapsFile.GetMapPixelID(x, y);
            if (mapDict.ContainsKey(id))
            {
                summaryOut = mapDict[id];
                return true;
            }

            summaryOut = new MapSummary();
            return false;
        }

        #endregion

        #region Private Methods

        private void UpdateClimate(int x, int y)
        {
            currentClimate = dfUnity.ContentReader.MapFileReader.GetClimateIndex(x, y);
            currentPolitic = dfUnity.ContentReader.MapFileReader.GetPoliticIndex(x, y);
            climateSettings = MapsFile.GetWorldClimateSettings(currentClimate);
        }

        private void EnumerateMaps()
        {
            mapDict = new Dictionary<int, MapSummary>();
            for (int region = 0; region < dfUnity.ContentReader.MapFileReader.RegionCount; region++)
            {
                DFRegion dfRegion = dfUnity.ContentReader.MapFileReader.GetRegion(region);
                for (int location = 0; location < dfRegion.LocationCount; location++)
                {
                    MapSummary summary = new MapSummary();
                    DFRegion.RegionMapTable mapTable = dfRegion.MapTable[location];
                    summary.ID = mapTable.MapId & 0x000fffff;
                    summary.RegionIndex = region;
                    summary.MapIndex = location;
                    summary.MapTable = mapTable;

                    if (mapDict.ContainsKey(summary.ID))
                        DaggerfallUnity.LogMessage("Duplicate MapTable.MapID found by PlayerGPS. This should not happen.");
                    else
                        mapDict.Add(summary.ID, summary);
                }
            }
        }

        private bool ReadyCheck()
        {
            // Ensure we have a DaggerfallUnity reference
            if (dfUnity == null)
            {
                if (!DaggerfallUnity.FindDaggerfallUnity(out dfUnity))
                {
                    DaggerfallUnity.LogMessage("PlayerGPS: Could not get DaggerfallUnity component.");
                    return false;
                }
            }

            // Do nothing if DaggerfallUnity not ready
            if (!dfUnity.IsReady)
            {
                DaggerfallUnity.LogMessage("PlayerGPS: DaggerfallUnity component is not ready. Have you set your Arena2 path?");
                return false;
            }

            // Build map lookup dictionary
            if (mapDict == null)
                EnumerateMaps();

            return true;
        }

        #endregion
    }
}