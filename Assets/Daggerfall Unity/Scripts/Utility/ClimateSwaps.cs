using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    public class ClimateSwaps
    {
        /// <summary>
        /// Converts an archive index to new climate and season.
        /// Will return same index if climate or season not supported.
        /// </summary>
        /// <param name="archive">Archive index of starting texture.</param>
        /// <param name="climate">Climate base to apply.</param>
        /// <param name="season">Climate season to apply</param>
        /// <returns>Archive index of new texture.</returns>
        public static int ApplyClimate(int archive, ClimateBases climate, ClimateSeason season)
        {
            // Get the base set of this archive
            bool supportsWinter, supportsRain;
            DFLocation.ClimateTextureSet climateSet = GetClimateSet(archive, out supportsWinter, out supportsRain);

            // Ignore non-climate textures
            if (climateSet == DFLocation.ClimateTextureSet.None)
                return archive;

            // Handle missing Swamp textures
            if (climate == ClimateBases.Swamp)
            {
                switch (climateSet)
                {
                    case DFLocation.ClimateTextureSet.Interior_TempleInt:
                    case DFLocation.ClimateTextureSet.Interior_MarbleFloors:
                        return archive;
                }
            }

            // Handle climate sets with missing winter textures
            if (climate == ClimateBases.Desert ||
                climate == ClimateBases.Swamp)
            {
                switch (climateSet)
                {
                    case DFLocation.ClimateTextureSet.Exterior_Castle:
                    case DFLocation.ClimateTextureSet.Exterior_MagesGuild:
                        supportsWinter = false;
                        break;
                }
            }

            // Calculate new index
            int climateIndex = 0;
            if (archive < 500)
            {
                climateIndex = (int)FromUnityClimateBase(climate) + (int)climateSet;
                if (season == ClimateSeason.Winter && supportsWinter)
                    climateIndex += (int)DFLocation.ClimateWeather.Winter;
                else if (season == ClimateSeason.Rain && supportsRain)
                    climateIndex += (int)DFLocation.ClimateWeather.Rain;
            }
            else
            {
                climateIndex = archive;
                if (season == ClimateSeason.Winter && supportsWinter)
                    climateIndex += (int)DFLocation.ClimateWeather.Winter;
            }

            return climateIndex;
        }

        /// <summary>
        /// Get the base climate set by itself.
        /// </summary>
        /// <param name="archive">Archive from which to derive set.</param>
        public static DFLocation.ClimateTextureSet GetClimateSet(int archive)
        {
            bool supportsWinter = false;
            bool supportsRain = false;
            return GetClimateSet(archive, out supportsWinter, out supportsRain);
        }

        /// <summary>
        /// Get the base climate set and support flags.
        /// </summary>
        /// <param name="archive">Archive from which to derive set.</param>
        /// <param name="supportsWinterOut">True if there is a winter version of this set.</param>
        /// <param name="supportsRainOut">True if there is a rain version of this set.</param>
        /// <returns>Derived ClimateSet.</returns>
        public static DFLocation.ClimateTextureSet GetClimateSet(int archive, out bool supportsWinterOut, out bool supportsRainOut)
        {
            supportsWinterOut = false;
            supportsRainOut = false;
            DFLocation.ClimateTextureSet set;

            // Handle nature sets
            if (archive > 499)
            {
                set = (DFLocation.ClimateTextureSet)archive;
                switch (set)
                {
                    // Nature sets without snow
                    case DFLocation.ClimateTextureSet.Nature_RainForest:
                    case DFLocation.ClimateTextureSet.Nature_SubTropical:
                    case DFLocation.ClimateTextureSet.Nature_Swamp:
                    case DFLocation.ClimateTextureSet.Nature_Desert:
                        return set;

                    // Nature sets with snow
                    case DFLocation.ClimateTextureSet.Nature_TemperateWoodland:
                    case DFLocation.ClimateTextureSet.Nature_WoodlandHills:
                    case DFLocation.ClimateTextureSet.Nature_HauntedWoodlands:
                    case DFLocation.ClimateTextureSet.Nature_Mountains:
                        supportsWinterOut = true;
                        return set;

                    default:
                        return DFLocation.ClimateTextureSet.None;
                }
            }

            // Get general set
            set = (DFLocation.ClimateTextureSet)(archive - (archive / 100) * 100);
            switch (set)
            {
                // Sets with winter and rain
                case DFLocation.ClimateTextureSet.Exterior_Terrain:
                    supportsWinterOut = true;
                    supportsRainOut = true;
                    break;

                // Sets with just winter
                case DFLocation.ClimateTextureSet.Exterior_Ruins:
                case DFLocation.ClimateTextureSet.Exterior_Castle:
                case DFLocation.ClimateTextureSet.Exterior_CityA:
                case DFLocation.ClimateTextureSet.Exterior_CityB:
                case DFLocation.ClimateTextureSet.Exterior_CityWalls:
                case DFLocation.ClimateTextureSet.Exterior_Farm:
                case DFLocation.ClimateTextureSet.Exterior_Fences:
                case DFLocation.ClimateTextureSet.Exterior_MagesGuild:
                case DFLocation.ClimateTextureSet.Exterior_Manor:
                case DFLocation.ClimateTextureSet.Exterior_MerchantHomes:
                case DFLocation.ClimateTextureSet.Exterior_TavernExteriors:
                case DFLocation.ClimateTextureSet.Exterior_TempleExteriors:
                case DFLocation.ClimateTextureSet.Exterior_Village:
                case DFLocation.ClimateTextureSet.Exterior_Roofs:
                    supportsWinterOut = true;
                    break;

                // Sets without winter or rain
                case DFLocation.ClimateTextureSet.Interior_PalaceInt:
                case DFLocation.ClimateTextureSet.Interior_CityInt:
                case DFLocation.ClimateTextureSet.Interior_CryptA:
                case DFLocation.ClimateTextureSet.Interior_CryptB:
                case DFLocation.ClimateTextureSet.Interior_DungeonsA:
                case DFLocation.ClimateTextureSet.Interior_DungeonsB:
                case DFLocation.ClimateTextureSet.Interior_DungeonsC:
                case DFLocation.ClimateTextureSet.Interior_DungeonsNEWCs:
                case DFLocation.ClimateTextureSet.Interior_FarmInt:
                case DFLocation.ClimateTextureSet.Interior_MagesGuildInt:
                case DFLocation.ClimateTextureSet.Interior_ManorInt:
                case DFLocation.ClimateTextureSet.Interior_MarbleFloors:
                case DFLocation.ClimateTextureSet.Interior_MerchantHomesInt:
                case DFLocation.ClimateTextureSet.Interior_Mines:
                case DFLocation.ClimateTextureSet.Interior_Caves:
                case DFLocation.ClimateTextureSet.Interior_Paintings:
                case DFLocation.ClimateTextureSet.Interior_TavernInt:
                case DFLocation.ClimateTextureSet.Interior_TempleInt:
                case DFLocation.ClimateTextureSet.Interior_VillageInt:
                case DFLocation.ClimateTextureSet.Interior_Sewer:
                case DFLocation.ClimateTextureSet.Doors:
                    break;
                    
                default:
                    return DFLocation.ClimateTextureSet.None;
            }

            // Found a matching set
            return set;
        }

        /// <summary>
        /// Convert DaggerfallUnity climate base to API equivalent.
        /// </summary>
        /// <param name="climate">ClimateBases.</param>
        /// <returns>DFLocation.ClimateBaseType.</returns>
        public static DFLocation.ClimateBaseType FromUnityClimateBase(ClimateBases climate)
        {
            switch (climate)
            {
                case ClimateBases.Desert:
                    return DFLocation.ClimateBaseType.Desert;
                case ClimateBases.Mountain:
                    return DFLocation.ClimateBaseType.Mountain;
                case ClimateBases.Temperate:
                    return DFLocation.ClimateBaseType.Temperate;
                case ClimateBases.Swamp:
                    return DFLocation.ClimateBaseType.Swamp;
                default:
                    return DFLocation.ClimateBaseType.None;
            }
        }

        /// <summary>
        /// Convert API climate base over to DaggerfallUnity equivalent.
        /// </summary>
        /// <param name="climate">DFLocation.ClimateBaseType.</param>
        /// <returns>ClimateBases.</returns>
        public static ClimateBases FromAPIClimateBase(DFLocation.ClimateBaseType climate)
        {
            switch (climate)
            {
                case DFLocation.ClimateBaseType.Desert:
                    return ClimateBases.Desert;
                case DFLocation.ClimateBaseType.Mountain:
                    return ClimateBases.Mountain;
                case DFLocation.ClimateBaseType.Temperate:
                    return ClimateBases.Temperate;
                case DFLocation.ClimateBaseType.Swamp:
                    return ClimateBases.Swamp;
                default:
                    return ClimateBases.Temperate;
            }
        }

        /// <summary>
        /// Convert API nature set to DaggerfallUnity equivalent.
        /// </summary>
        /// <param name="set"></param>
        /// <returns>ClimateNatureSets.</returns>
        public static ClimateNatureSets FromAPITextureSet(DFLocation.ClimateTextureSet set)
        {
            switch (set)
            {
                case DFLocation.ClimateTextureSet.Nature_RainForest:
                    return ClimateNatureSets.RainForest;
                case DFLocation.ClimateTextureSet.Nature_SubTropical:
                    return ClimateNatureSets.SubTropical;
                case DFLocation.ClimateTextureSet.Nature_Swamp:
                    return ClimateNatureSets.Swamp;
                case DFLocation.ClimateTextureSet.Nature_Desert:
                    return ClimateNatureSets.Desert;
                case DFLocation.ClimateTextureSet.Nature_TemperateWoodland:
                    return ClimateNatureSets.TemperateWoodland;
                case DFLocation.ClimateTextureSet.Nature_WoodlandHills:
                    return ClimateNatureSets.WoodlandHills;
                case DFLocation.ClimateTextureSet.Nature_HauntedWoodlands:
                    return ClimateNatureSets.HauntedWoodlands;
                case DFLocation.ClimateTextureSet.Nature_Mountains:
                    return ClimateNatureSets.Mountains;
                default:
                    return ClimateNatureSets.TemperateWoodland;
            }
        }

        public static int GetNatureArchive(ClimateNatureSets natureSet, ClimateSeason climateSeason)
        {
            // Get base set
            int archive;
            switch (natureSet)
            {
                case ClimateNatureSets.RainForest:
                    archive = 500;
                    break;
                case ClimateNatureSets.SubTropical:
                    archive = 501;
                    break;
                case ClimateNatureSets.Swamp:
                    archive = 502;
                    break;
                case ClimateNatureSets.Desert:
                    archive = 503;
                    break;
                case ClimateNatureSets.TemperateWoodland:
                    archive = 504;
                    break;
                case ClimateNatureSets.WoodlandHills:
                    archive = 506;
                    break;
                case ClimateNatureSets.HauntedWoodlands:
                    archive = 508;
                    break;
                case ClimateNatureSets.Mountains:
                    archive = 510;
                    break;
                default:
                    archive = 504;
                    break;
            }

            // Winter modifier
            if (climateSeason == ClimateSeason.Winter)
            {
                // Only certain sets have a winter archive
                switch (natureSet)
                {
                    case ClimateNatureSets.TemperateWoodland:
                    case ClimateNatureSets.WoodlandHills:
                    case ClimateNatureSets.HauntedWoodlands:
                    case ClimateNatureSets.Mountains:
                        archive += 1;
                        break;
                }
            }

            return archive;
        }

        /// <summary>
        /// Get ground archive based on climate.
        /// </summary>
        /// <param name="climateBase">Climate base.</param>
        /// <param name="climateSeason">Season.</param>
        /// <returns>Ground archive matching climate and season.</returns>
        public static int GetGroundArchive(ClimateBases climateBase, ClimateSeason climateSeason)
        {
            // Apply climate
            int archive;
            switch (climateBase)
            {
                case ClimateBases.Desert:
                    archive = 2;
                    break;
                case ClimateBases.Mountain:
                    archive = 102;
                    break;
                case ClimateBases.Temperate:
                    archive = 302;
                    break;
                case ClimateBases.Swamp:
                    archive = 402;
                    break;
                default:
                    archive = 302;
                    break;
            }

            // Modify for season
            switch (climateSeason)
            {
                case ClimateSeason.Winter:
                    archive += 1;
                    break;
                case ClimateSeason.Rain:
                    archive += 2;
                    break;
            }

            return archive;
        }
    }
}