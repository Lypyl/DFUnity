using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Basic image processing for textures.
    /// </summary>
    public class ImageProcessing
    {
        /// <summary>
        /// Creates a blended border around transparent textures.
        /// Removes dark edges from billboards.
        /// </summary>
        /// <param name="colors">Source image.</param>
        /// <param name="size">Image size.</param>
        public static void DilateColors(ref Color32[] colors, DFSize size)
        {
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    Color32 color = ReadColor(ref colors, ref size, x, y);
                    if (color.a != 0)
                    {
                        MixColor(ref colors, ref size, color, x - 1, y - 1);
                        MixColor(ref colors, ref size, color, x, y - 1);
                        MixColor(ref colors, ref size, color, x + 1, y - 1);
                        MixColor(ref colors, ref size, color, x - 1, y);
                        MixColor(ref colors, ref size, color, x + 1, y);
                        MixColor(ref colors, ref size, color, x - 1, y + 1);
                        MixColor(ref colors, ref size, color, x, y + 1);
                        MixColor(ref colors, ref size, color, x + 1, y + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Copies texture to all eight positions around itself.
        /// Reduces bleeding at low mipmap levels for atlased tiles (e.g. ground textures).
        /// </summary>
        /// <param name="colors">Source image.</param>
        /// <param name="size">Image size.</param>
        /// <param name="border">Border width.</param>
        public static void CopyToOppositeBorder(ref Color32[] colors, DFSize size, int border)
        {
            // Copy left-right
            for (int y = border; y < size.Height - border; y++)
            {
                int ypos = y * size.Width;
                int il = ypos + border;
                int ir = ypos + size.Width - border * 2;
                for (int x = 0; x < border; x++)
                {
                    colors[ypos + x] = colors[ir + x];
                    colors[ypos + size.Width - border + x] = colors[il + x];
                }
            }

            // Copy top-bottom
            for (int y = 0; y < border; y++)
            {
                int ypos1 = y * size.Width;
                int ypos2 = (y + size.Height - border) * size.Width;

                int it = (border + y) * size.Width;
                int ib = (y + size.Height - border * 2) * size.Width;
                for (int x = 0; x < size.Width; x++)
                {
                    colors[ypos1 + x] = colors[ib + x];
                    colors[ypos2 + x] = colors[it + x];
                }
            }
        }

        /// <summary>
        /// Tint a weapon image based on metal type.
        /// </summary>
        /// <param name="srcBitmap">Source weapon image.</param>
        /// <param name="size">Image size.</param>
        /// <param name="metalType">Metal type for tint.</param>
        public static void TintWeaponImage(ref DFBitmap srcBitmap, MetalTypes metalType)
        {
            byte[] swaps = GetMetalColors(metalType);

            int rowPos;
            for (int y = 0; y < srcBitmap.Height; y++)
            {
                rowPos = y * srcBitmap.Width;
                for (int x = 0; x < srcBitmap.Width; x++)
                {
                    byte index = srcBitmap.Data[rowPos + x];
                    if (index >= 0x70 && index <= 0x7f)
                    {
                        int offset = index - 0x70;
                        srcBitmap.Data[rowPos + x] = swaps[offset];
                    }
                }
            }
        }

        public static byte[] GetMetalColors(MetalTypes metalType)
        {
            byte[] indices;
            switch (metalType)
            {
                case MetalTypes.Iron:
                    indices = new byte[] { 0x77, 0x78, 0x57, 0x79, 0x58, 0x59, 0x7A, 0x5A, 0x7B, 0x5B, 0x7C, 0x5C, 0x7D, 0x5D, 0x5E, 0x5F };
                    break;
                case MetalTypes.Steel:
                    indices = new byte[] { 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F };
                    break;
                case MetalTypes.Silver:
                    indices = new byte[] { 0xE0, 0x70, 0x50, 0x71, 0x51, 0x72, 0x73, 0x52, 0x74, 0x53, 0x75, 0x54, 0x55, 0x56, 0x57, 0x58 };
                    break;
                case MetalTypes.Elven:
                    indices = new byte[] { 0xE0, 0x70, 0x50, 0x71, 0x51, 0x72, 0x73, 0x52, 0x74, 0x53, 0x75, 0x54, 0x55, 0x56, 0x57, 0x58 };
                    break;
                case MetalTypes.Dwarven:
                    indices = new byte[] { 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F };
                    break;
                case MetalTypes.Mithril:
                    indices = new byte[] { 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE };
                    break;
                case MetalTypes.Adamantium:
                    indices = new byte[] { 0x5A, 0x5B, 0x7C, 0x5C, 0x7D, 0x5D, 0x7E, 0x5E, 0x7F, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE };
                    break;
                case MetalTypes.Ebony:
                    indices = new byte[] { 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE };
                    break;
                case MetalTypes.Orcish:
                    indices = new byte[] { 0xA2, 0xA3, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD };
                    break;
                case MetalTypes.Daedric:
                    indices = new byte[] { 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE };
                    break;
                default:
                    indices = new byte[] { 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F };
                    break;
            }

            return indices;
        }

        #region Private Methods

        private static void MixColor(ref Color32[] colors, ref DFSize size, Color32 src, int x, int y)
        {
            // Handle outside of bounds
            if (x < 0 || y < 0 || x > size.Width - 1 || y > size.Height - 1)
                return;

            // Get destination pixel colour and ensure it has empty alpha
            Color32 dst = ReadColor(ref colors, ref size, x, y);
            if (dst.a != 0)
                return;

            // Get count for averaging
            int count = 1;
            if (dst != Color.clear)
                count = 2;

            // Mix source colour with destination
            Vector3 avg = new Vector3(
                src.r + dst.r,
                src.g + dst.g,
                src.b + dst.b) / count;

            // Assign new colour to destination
            colors[y * size.Width + x] = new Color32((byte)avg.x, (byte)avg.y, (byte)avg.z, 0);
        }

        private static Color32 ReadColor(ref Color32[] colors, ref DFSize size, int x, int y)
        {
            // Handle outside of bounds
            if (x < 0 || y < 0 || x > size.Width - 1 || y > size.Height - 1)
                return Color.clear;

            return colors[y * size.Width + x];
        }

        #endregion
    }
}