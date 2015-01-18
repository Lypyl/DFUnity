// Project:         Daggerfall Unity -- A game built with Daggerfall Tools For Unity
// Description:     This is a modified version of a script provided by Daggerfall Tools for Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Project Page:    https://github.com/DKoestler/DFUnity -- https://code.google.com/p/daggerfall-unity/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;

namespace Daggerfall { 
    /// <summary>
    /// Tracks player position in world space.
    /// </summary>
    public class PlayerGPS : MonoBehaviour
    {
        // Default location is outside Privateer's Hold
        [Range(0, 32735232)]
        public int WorldX;                      // Player X coordinate in Daggerfall world units
        [Range(0, 16351232)]
        public int WorldZ;                      // Player Z coordinate in Daggerfall world units

        DaggerfallUnity dfUnity;
        int lastMapPixelX = -1;
        int lastMapPixelY = -1;
        int currentClimate;
        int currentPolitic;
        DFLocation.ClimateSettings climateSettings;

        /// <summary>
        /// Gets current player map pixel.
        /// </summary>
        public DFPosition CurrentMapPixel
        {
            get { return MapsFile.WorldCoordToMapPixel(WorldX, WorldZ); }
        }

        /// <summary>
        /// Gets climate index based on player world position.
        /// </summary>
        public int CurrentClimate
        {
            get { return currentClimate; }
        }

        /// <summary>
        /// Gets political index based on player world position.
        /// </summary>
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

            // Update local world information whenever player map pixel changes
            DFPosition pos = CurrentMapPixel;
            if (pos.X != lastMapPixelX || pos.Y != lastMapPixelY)
            {
                UpdateWorldInfo(pos.X, pos.Y);
                lastMapPixelX = pos.X;
                lastMapPixelY = pos.Y;
            }
        }

        #region Private Methods

        private void UpdateWorldInfo(int x, int y)
        {
            currentClimate = dfUnity.ContentReader.MapFileReader.GetClimateIndex(x, y);
            currentPolitic = dfUnity.ContentReader.MapFileReader.GetPoliticIndex(x, y);
            climateSettings = MapsFile.GetWorldClimateSettings(currentClimate);
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

            return true;
        }

        #endregion
    }
}