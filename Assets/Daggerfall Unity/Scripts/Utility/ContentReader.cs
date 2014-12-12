using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// General content interface between DaggerfallConnect API classes and Unity.
    /// </summary>
    public class ContentReader
    {
        bool isReady = false;
        string arena2Path;
        
        BlocksFile blockFileReader;
        MapsFile mapFileReader;
        MonsterFile monsterFileReader;
        ConfFile confFileReader;

        public bool IsReady
        {
            get { return isReady; }
        }

        public BlocksFile BlockFileReader
        {
            get { return blockFileReader; }
        }

        public MapsFile MapFileReader
        {
            get { return mapFileReader; }
        }

        public MonsterFile MonsterFileReader
        {
            get { return monsterFileReader; }
        }

        #region Constructors

        public ContentReader(string arena2Path, DaggerfallUnity dfUnity)
        {
            this.arena2Path = arena2Path;
            SetupReaders();
        }

        #endregion

        #region Blocks & Locations

        /// <summary>
        /// Attempts to get a Daggerfall block from BLOCKS.BSA.
        /// </summary>
        /// <param name="name">Name of block.</param>
        /// <param name="blockOut">DFBlock data out.</param>
        /// <returns>True if successful.</returns>
        public bool GetBlock(string name, out DFBlock blockOut)
        {
            blockOut = new DFBlock();

            if (!isReady)
                return false;

            // Get block data
            blockOut = blockFileReader.GetBlock(name);
            if (blockOut.Type == DFBlock.BlockTypes.Unknown)
            {
                DaggerfallUnity.LogMessage(string.Format("Unknown block '{0}'.", name), true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to get a Daggerfall location from MAPS.BSA.
        /// </summary>
        /// <param name="regionName">Name of region.</param>
        /// <param name="locationName">Name of location.</param>
        /// <param name="locationOut">DFLocation data out.</param>
        /// <returns>True if successful.</returns>
        public bool GetLocation(string regionName, string locationName, out DFLocation locationOut)
        {
            locationOut = new DFLocation();

            if (!isReady)
                return false;

            // Get location data
            locationOut = mapFileReader.GetLocation(regionName, locationName);
            if (!locationOut.Loaded)
            {
                DaggerfallUnity.LogMessage(string.Format("Unknown location RegionName='{0}', LocationName='{1}'.", regionName, locationName), true);
                return false;
            }

            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Setup API file readers.
        /// </summary>
        private void SetupReaders()
        {
            if (blockFileReader == null) { 
                blockFileReader = new BlocksFile(Path.Combine(arena2Path, BlocksFile.Filename), FileUsage.UseMemory, true);
            }
            if (mapFileReader == null) { 
                mapFileReader = new MapsFile(Path.Combine(arena2Path, MapsFile.Filename), FileUsage.UseMemory, true);
            }
            if (monsterFileReader == null) { 
                monsterFileReader = new MonsterFile(Path.Combine(arena2Path, MonsterFile.Filename), FileUsage.UseMemory, true);
            }
            if (confFileReader == null) {
                // TODO: DEBUG: Remove messages
                DaggerfallWorkshop.Game.DevConsole.displayText("Loading a new conf file reader");
                confFileReader = new ConfFile(ConfFile.Filename, FileUsage.UseDisk, false);
                DaggerfallWorkshop.Game.DevConsole.displayText("Finished loading the conf file reader");
            }
            isReady = true;
        }

        #endregion
    }
}
