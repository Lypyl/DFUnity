using UnityEngine;
using System.IO;
using System.Collections;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Renders Daggerfall's small compass.
    /// Should be attached to player camera.
    /// </summary>
    public class PlayerCompass : MonoBehaviour
    {
        DaggerfallUnity dfUnity;
        Camera mainCamera;
        Texture2D compassTexture;
        Texture2D compassBoxTexture;

        int scale = 2;
        bool assetsLoaded = false;

        void Start()
        {
            // Reference components
            dfUnity = DaggerfallUnity.Instance;
            mainCamera = Camera.main;

            // Adjust scale based on resolution
            if (Screen.currentResolution.height > 1080)
                scale = 3;
            if (Screen.currentResolution.height > 1440)
                scale = 4;
        }

        void Update()
        {
            if (!assetsLoaded)
                LoadAssets();
        }

        void OnGUI()
        {
            if (!compassTexture || !compassBoxTexture)
                return;

            // Redraw compass
            if (Event.current.type.Equals(EventType.Repaint))
            {
                DrawCompass();
            }
        }

        #region Private Methods

        void LoadAssets()
        {
            const string compassFilename = "COMPASS.IMG";
            const string compassBoxFilename = "COMPBOX.IMG";

            if (!dfUnity.IsReady)
                return;

            if (!compassTexture)
            {
                ImgFile file = new ImgFile(Path.Combine(dfUnity.Arena2Path, compassFilename), FileUsage.UseMemory, true);
                file.LoadPalette(Path.Combine(dfUnity.Arena2Path, file.PaletteName));
                compassTexture = GetTextureFromImg(file);
            }

            if (!compassBoxTexture)
            {
                ImgFile file = new ImgFile(Path.Combine(dfUnity.Arena2Path, compassBoxFilename), FileUsage.UseMemory, true);
                file.LoadPalette(Path.Combine(dfUnity.Arena2Path, file.PaletteName));
                compassBoxTexture = GetTextureFromImg(file);
            }

            assetsLoaded = true;
        }

        Texture2D GetTextureFromImg(ImgFile img)
        {
            DFBitmap bitmap = img.GetDFBitmap();
            Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.ARGB32, false);
            texture.SetPixels32(img.GetColors32(ref bitmap, 0));
            texture.Apply(false, true);

            return texture;
        }

        void DrawCompass()
        {
            const int boxOutlineSize = 2;

            // Calculate displacement
            float percent = mainCamera.transform.eulerAngles.y / 360f;
            int scroll = (int)((float)258 * percent);

            // Compass box rect
            Rect compassBoxRect = new Rect();
            compassBoxRect.x = Screen.width - (compassBoxTexture.width * scale);
            compassBoxRect.y = Screen.height - (compassBoxTexture.height * scale);
            compassBoxRect.width = compassBoxTexture.width * scale;
            compassBoxRect.height = compassBoxTexture.height * scale;

            // Compass strip source
            Rect compassSrcRect = new Rect();
            compassSrcRect.xMin = scroll / (float)compassTexture.width;
            compassSrcRect.yMin = 0;
            compassSrcRect.xMax = compassSrcRect.xMin + 64f / (float)compassTexture.width;
            compassSrcRect.yMax = 1;

            // Compass strip destination
            Rect compassDstRect = new Rect();
            compassDstRect.x = compassBoxRect.x + boxOutlineSize * scale;
            compassDstRect.y = compassBoxRect.y + boxOutlineSize * scale;
            compassDstRect.width = compassBoxRect.width - (boxOutlineSize * 2) * scale;
            compassDstRect.height = compassTexture.height * scale;

            GUI.DrawTexture(compassBoxRect, compassBoxTexture, ScaleMode.StretchToFill, false);
            GUI.DrawTextureWithTexCoords(compassDstRect, compassTexture, compassSrcRect, true);
        }

        #endregion
    }
}