using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Helper methods for terrain generation.
    /// </summary>
    public static class TerrainHelper
    {
        // Terrain setup constants
        public const int terrainTileDim = 128;                          // Terrain tile dimension is 128x128 ground tiles
        public const int terrainSampleDim = terrainTileDim + 1;         // Terrain height sample dimension has 1 extra point for end vertex

        // Maximum terrain height is determined by scaled max input values
        // This can be further increased by global scale on terrain itself
        // Formula is (128 * baseHeightScale) + (128 * noiseMapScale) + extraNoiseScale
        // Do not change these unless necessary, use DaggerTerrain.TerrainScale from editor instead
        public const float baseHeightScale = 8f;
        public const float noiseMapScale = 4f;
        public const float extraNoiseScale = 1.5f;
        public const float maxTerrainHeight = 1538f;

        // Elevation of ocean and beach
        public const float scaledOceanElevation = 3.5f * baseHeightScale;
        public const float scaledBeachElevation = 5 * baseHeightScale;

        // Ranges and defaults for editor
        // Map pixel ranges are slightly smaller to allow for interpolation of neighbours
        public const int minMapPixelX = 3;
        public const int minMapPixelY = 3;
        public const int maxMapPixelX = 998;
        public const int maxMapPixelY = 498;
        public const int defaultMapPixelX = 207;
        public const int defaultMapPixelY = 213;
        public const float minTerrainScale = 1.0f;
        public const float maxTerrainScale = 6.0f;
        public const float defaultTerrainScale = 2.0f;

        /// <summary>
        /// Gets map pixel data for any location in world.
        /// </summary>
        public static MapPixelData GetMapPixelData(ContentReader contentReader, int mapPixelX, int mapPixelY)
        {
            // Read general data from world maps
            int worldHeight = contentReader.WoodsFileReader.GetHeightMapValue(mapPixelX, mapPixelY);
            int worldClimate = contentReader.MapFileReader.GetClimateIndex(mapPixelX, mapPixelY);
            int worldPolitic = contentReader.MapFileReader.GetPoliticIndex(mapPixelX, mapPixelY);

            // Get location if present
            int id = -1, regionIndex = -1, mapIndex = -1;
            string locationName = string.Empty;
            ContentReader.MapSummary mapSummary = new ContentReader.MapSummary();
            bool hasLocation = contentReader.HasLocation(mapPixelX, mapPixelY, out mapSummary);
            if (hasLocation)
            {
                id = mapSummary.ID;
                regionIndex = mapSummary.RegionIndex;
                mapIndex = mapSummary.MapIndex;
                DFLocation location = contentReader.MapFileReader.GetLocation(regionIndex, mapIndex);
                locationName = location.Name;
            }

            // Create map pixel data
            MapPixelData mapPixel = new MapPixelData()
            {
                inWorld = true,
                mapPixelX = mapPixelX,
                mapPixelY = mapPixelY,
                worldHeight = worldHeight,
                worldClimate = worldClimate,
                worldPolitic = worldPolitic,
                hasLocation = hasLocation,
                mapRegionIndex = regionIndex,
                mapLocationIndex = mapIndex,
                locationID = id,
                locationName = locationName,
            };

            return mapPixel;
        }

        /// <summary>
        /// Generate initial samples from any map pixel coordinates in world range.
        /// Also sets location height in mapPixelData for location positioning.
        /// </summary>
        public static void GenerateSamples(ContentReader contentReader, ref MapPixelData mapPixel)
        {
            // Divisor ensures continuous 0-1 range of tile samples
            float div = (float)terrainTileDim / 3f;

            // Read neighbouring height samples for this map pixel
            int mx = mapPixel.mapPixelX;
            int my = mapPixel.mapPixelY;
            byte[,] shm = contentReader.WoodsFileReader.GetHeightMapValuesRange(mx, my, 4);
            byte[,] lhm = contentReader.WoodsFileReader.GetLargeHeightMapValuesRange(mx, my, 3);

            // Extract height samples for all chunks
            float averageHeight = 0;
            float baseHeight, noiseHeight;
            float x1, x2, x3, x4;
            int dim = terrainSampleDim;
            mapPixel.samples = new WorldSample[dim * dim];
            for (int y = 0; y < dim; y++)
            {
                for (int x = 0; x < dim; x++)
                {
                    float rx = (float)x / div;
                    float ry = (float)y / div;
                    int ix = Mathf.FloorToInt(rx);
                    int iy = Mathf.FloorToInt(ry);
                    float sfracx = (float)x / (float)(dim - 1);
                    float sfracy = (float)y / (float)(dim - 1);
                    float fracx = (float)(x - ix * div) / div;
                    float fracy = (float)(y - iy * div) / div;
                    float scaledHeight = 0;

                    // Bicubic sample small height map for base terrain elevation
                    x1 = CubicInterpolator(shm[0, 0], shm[1, 0], shm[2, 0], shm[3, 0], sfracx);
                    x2 = CubicInterpolator(shm[0, 1], shm[1, 1], shm[2, 1], shm[3, 1], sfracx);
                    x3 = CubicInterpolator(shm[0, 2], shm[1, 2], shm[2, 2], shm[3, 2], sfracx);
                    x4 = CubicInterpolator(shm[0, 3], shm[1, 3], shm[2, 3], shm[3, 3], sfracx);
                    baseHeight = CubicInterpolator(x1, x2, x3, x4, sfracy);
                    scaledHeight += baseHeight * baseHeightScale;

                    // Bicubic sample large height map for noise mask over terrain features
                    x1 = CubicInterpolator(lhm[ix, iy], lhm[ix + 1, iy], lhm[ix + 2, iy], lhm[ix + 3, iy], fracx);
                    x2 = CubicInterpolator(lhm[ix, iy + 1], lhm[ix + 1, iy + 1], lhm[ix + 2, iy + 1], lhm[ix + 3, iy + 1], fracx);
                    x3 = CubicInterpolator(lhm[ix, iy + 2], lhm[ix + 1, iy + 2], lhm[ix + 2, iy + 2], lhm[ix + 3, iy + 2], fracx);
                    x4 = CubicInterpolator(lhm[ix, iy + 3], lhm[ix + 1, iy + 3], lhm[ix + 2, iy + 3], lhm[ix + 3, iy + 3], fracx);
                    noiseHeight = CubicInterpolator(x1, x2, x3, x4, fracy);
                    scaledHeight += noiseHeight * noiseMapScale;

                    // Additional noise mask for small terrain features at ground level
                    float latitude = mapPixel.mapPixelX * MapsFile.WorldMapTileDim + x;
                    float longitude = MapsFile.MaxWorldTileCoordZ - mapPixel.mapPixelY * MapsFile.WorldMapTileDim + y;
                    float extraNoise = GetNoise(contentReader, latitude, longitude, 0.1f, 0.5f, 0.5f, 1) * extraNoiseScale;
                    scaledHeight += extraNoise * extraNoiseScale;

                    // Clamp lower values to ocean elevation
                    if (scaledHeight < scaledOceanElevation)
                        scaledHeight = scaledOceanElevation;

                    // Accumulate average height
                    averageHeight += scaledHeight;

                    // Set sample
                    mapPixel.samples[y * dim + x] = new WorldSample()
                    {
                        scaledHeight = scaledHeight,
                        record = 2,
                    };
                }
            }

            // Average height is passed back for locations
            mapPixel.locationHeight = averageHeight /= (float)(dim * dim);
        }

        // Clear all sample tiles to same base index
        public static void ClearSampleTiles(ref MapPixelData mapPixel, byte record)
        {
            for (int i = 0; i < mapPixel.samples.Length; i++)
            {
                mapPixel.samples[i].record = record;
            }
        }

        // Set texture and height data for city tiles
        public static void SetLocationTiles(ContentReader contentReader, ref MapPixelData mapPixel)
        {
            const int tileDim = 16;
            const int chunkDim = 8;

            // Get location
            DFLocation location = contentReader.MapFileReader.GetLocation(mapPixel.mapRegionIndex, mapPixel.mapLocationIndex);

            // Centre location tiles inside terrain area
            int startX = ((chunkDim * tileDim) - location.Exterior.ExteriorData.Width * tileDim) / 2;
            int startY = ((chunkDim * tileDim) - location.Exterior.ExteriorData.Height * tileDim) / 2;

            // Full 8x8 locations have "terrain blend space" around walls to smooth down random terrain towards flat area.
            // This is indicated by texture index > 55 (ground texture range is 0-55), larger values indicate blend space.
            // We need to know rect of actual city area so we can use blend space outside walls.
            int xmin = TerrainHelper.terrainSampleDim, ymin = TerrainHelper.terrainSampleDim;
            int xmax = 0, ymax = 0;

            // Iterate blocks of this location
            for (int blockY = 0; blockY < location.Exterior.ExteriorData.Height; blockY++)
            {
                for (int blockX = 0; blockX < location.Exterior.ExteriorData.Width; blockX++)
                {
                    // Get block data
                    DFBlock block;
                    string blockName = contentReader.MapFileReader.GetRmbBlockName(ref location, blockX, blockY);
                    if (!contentReader.GetBlock(blockName, out block))
                        continue;

                    // Copy ground tile info
                    for (int tileY = 0; tileY < tileDim; tileY++)
                    {
                        for (int tileX = 0; tileX < tileDim; tileX++)
                        {
                            DFBlock.RmbGroundTiles tile = block.RmbBlock.FldHeader.GroundData.GroundTiles[tileX, (tileDim - 1) - tileY];
                            int xpos = startX + blockX * tileDim + tileX;
                            int ypos = startY + blockY * tileDim + tileY;
                            int offset = (ypos * TerrainHelper.terrainSampleDim) + xpos;

                            int record = tile.TextureRecord;
                            if (tile.TextureRecord < 56)
                            {
                                // Track interior bounds of location tiled area
                                if (xpos < xmin) xmin = xpos;
                                if (xpos > xmax) xmax = xpos;
                                if (ypos < ymin) ymin = ypos;
                                if (ypos > ymax) ymax = ypos;

                                // Store texture data from block and flatten to location height
                                mapPixel.samples[offset].record = record;
                                mapPixel.samples[offset].flip = tile.IsFlipped;
                                mapPixel.samples[offset].rotate = tile.IsRotated;
                                mapPixel.samples[offset].location = true;
                                mapPixel.samples[offset].scaledHeight = mapPixel.locationHeight;
                            }
                        }
                    }
                }
            }

            // Update location rect
            Rect locationRect = new Rect();
            locationRect.xMin = xmin;
            locationRect.xMax = xmax;
            locationRect.yMin = ymin;
            locationRect.yMax = ymax;
            mapPixel.locationRect = locationRect;
        }

        // Flattens location terrain and blends flat area with surrounding terrain
        // Not entirely happy with this, need to revisit later
        public static void FlattenLocationTerrain(ref MapPixelData mapPixel)
        {
            // Get range between bounds of sample data and interior location rect
            // The location rect is always smaller than the sample area
            float leftRange = 1f / (mapPixel.locationRect.xMin);
            float topRange = 1f / (mapPixel.locationRect.yMin);
            float rightRange = 1f / (terrainSampleDim - mapPixel.locationRect.xMax);
            float bottomRange = 1f / (terrainSampleDim - mapPixel.locationRect.yMax);

            float strength = 0;
            float u, v;
            for (int y = 1; y < terrainSampleDim - 1; y++)
            {
                for (int x = 1; x < terrainSampleDim - 1; x++)
                {
                    // Create a height scale from location to edge of terrain using
                    // linear interpolation on straight edges and bilinear in corners
                    if (x < mapPixel.locationRect.xMin && y > mapPixel.locationRect.yMin && y < mapPixel.locationRect.yMax)
                    {
                        strength = x * leftRange;
                    }
                    else if (x > mapPixel.locationRect.xMax && y > mapPixel.locationRect.yMin && y < mapPixel.locationRect.yMax)
                    {
                        strength = (TerrainHelper.terrainSampleDim - x) * rightRange;
                    }
                    else if (y < mapPixel.locationRect.yMin && x > mapPixel.locationRect.xMin && x < mapPixel.locationRect.xMax)
                    {
                        strength = y * topRange;
                    }
                    else if (y > mapPixel.locationRect.yMax && x > mapPixel.locationRect.xMin && x < mapPixel.locationRect.xMax)
                    {
                        strength = (TerrainHelper.terrainSampleDim - y) * bottomRange;
                    }
                    else if (x <= mapPixel.locationRect.xMin && y <= mapPixel.locationRect.yMin)
                    {
                        u = x * leftRange;
                        v = y * topRange;
                        strength = TerrainHelper.BilinearInterpolator(0, 0, 0, 1, u, v);
                    }
                    else if (x >= mapPixel.locationRect.xMax && y <= mapPixel.locationRect.yMin)
                    {
                        u = (TerrainHelper.terrainSampleDim - x) * rightRange;
                        v = y * topRange;
                        strength = TerrainHelper.BilinearInterpolator(0, 0, 0, 1, u, v);
                    }
                    else if (x <= mapPixel.locationRect.xMin && y >= mapPixel.locationRect.yMax)
                    {
                        u = x * leftRange;
                        v = (TerrainHelper.terrainSampleDim - y) * bottomRange;
                        strength = TerrainHelper.BilinearInterpolator(0, 0, 0, 1, u, v);
                    }
                    else if (x >= mapPixel.locationRect.xMax && y >= mapPixel.locationRect.yMax)
                    {
                        u = (TerrainHelper.terrainSampleDim - x) * rightRange;
                        v = (TerrainHelper.terrainSampleDim - y) * bottomRange;
                        strength = TerrainHelper.BilinearInterpolator(0, 0, 0, 1, u, v);
                    }

                    int offset = y * TerrainHelper.terrainSampleDim + x;
                    float curHeight = mapPixel.samples[offset].scaledHeight;
                    if (!mapPixel.samples[offset].location)
                    {
                        mapPixel.samples[offset].scaledHeight = (mapPixel.locationHeight * strength) + (curHeight * (1 - strength));
                    }
                    else
                    {
                        mapPixel.samples[offset].scaledHeight = mapPixel.locationHeight;
                    }
                }
            }
        }

        /// <summary>
        /// Get height value at coordinates.
        /// </summary>
        public static float GetHeight(ref WorldSample[] samples, int x, int y)
        {
            return samples[y * terrainSampleDim + x].scaledHeight;
        }

        /// <summary>
        /// Set height value at coordinates.
        /// </summary>
        public static void SetHeight(ref WorldSample[] samples, int x, int y, float height)
        {
            samples[y * terrainSampleDim + x].scaledHeight = height;
        }

        #region Private Methods

        // Bilinear interpolation of values
        private static float BilinearInterpolator(float valx0y0, float valx0y1, float valx1y0, float valx1y1, float u, float v)
        {
            float result =
                        (1 - u) * ((1 - v) * valx0y0 +
                        v * valx0y1) +
                        u * ((1 - v) * valx1y0 +
                        v * valx1y1);

            return result;
        }

        // Cubic interpolation of values
        private static float CubicInterpolator(float v0, float v1, float v2, float v3, float fracy)
        {
            float A = (v3 - v2) - (v0 - v1);
            float B = (v0 - v1) - A;
            float C = v2 - v0;
            float D = v1;

            return A * (fracy * fracy * fracy) + B * (fracy * fracy) + C * fracy + D;
            //return A * Mathf.Pow(fracy, 3) + B * Mathf.Pow(fracy, 2) + C * fracy + D;
        }

        // Get noise sample at coordinates
        private static float GetNoise(
            ContentReader reader,
            float x,
            float y,
            float frequency,
            float amplitude,
            float persistance,
            int octaves)
        {
            float finalValue = 0f;
            for (int i = 0; i < octaves; ++i)
            {
                finalValue += reader.Noise.Generate(x * frequency, y * frequency) * amplitude;
                frequency *= 2.0f;
                amplitude *= persistance;
            }

            return Mathf.Clamp(finalValue, -1, 1);
        }

        #endregion
    }
}