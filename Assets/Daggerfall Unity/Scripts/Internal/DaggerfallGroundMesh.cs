using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class DaggerfallGroundMesh : MonoBehaviour
    {
        [SerializeField]
        private GroundSummary summary = new GroundSummary();

        public GroundSummary Summary
        {
            get { return summary; }
        }

        [Serializable]
        public struct GroundSummary
        {
            public ClimateBases climate;
            public ClimateSeason season;
            public int archive;
            public Rect[] rects;
        }

        /// <summary>
        /// Set ground climate by texture archive index.
        /// </summary>
        /// <param name="dfUnity">DaggerfallUnity singleton. Required for content readers and settings.</param>
        /// <param name="archive">Texture archive index.</param>
        /// <param name="season">Season to set.</param>
        public void SetClimate(DaggerfallUnity dfUnity, int archive, ClimateSeason season)
        {
            // Get atlas material
            Rect[] rects;
            RecordIndex[] indices;
            Material atlasMaterial = dfUnity.MaterialReader.GetMaterialAtlas(
               archive,
               -1,
               8,
               2048,
               out rects,
               out indices,
               64,
               false,
               1,
               true);

            // Assign new season
            summary.archive = archive;
            summary.season = season;
            summary.rects = rects;
            renderer.sharedMaterial = atlasMaterial;
        }

        /// <summary>
        /// Set ground climate.
        /// </summary>
        /// <param name="dfUnity">DaggerfallUnity singleton. Required for content readers and settings.</param>
        /// <param name="climate">Climate to set.</param>
        /// <param name="season">Season to set.</param>
        public void SetClimate(DaggerfallUnity dfUnity, ClimateBases climate, ClimateSeason season)
        {
            int archive = ClimateSwaps.GetGroundArchive(climate, season);
            SetClimate(dfUnity, archive, season);
            summary.climate = climate;
        }
    }
}