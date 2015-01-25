// Project:         Daggerfall Unity -- A game built with Daggerfall Tools For Unity
// Description:     This is a modified version of a script provided by Daggerfall Tools for Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Project Page:    https://github.com/EBFEh/DFUnity -- https://code.google.com/p/daggerfall-unity/

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop;

namespace Daggerfall.Gameplay
{
    /// <summary>
    /// Just drawing a basic crosshair using OnGUI().
    /// </summary>
    public class FPSCrosshair : MonoBehaviour
    {
        public Texture CrosshairTexture;

        void OnGUI()
        {
            if (CrosshairTexture != null)
            {
                GUI.color = new Color(1, 1, 1, 0.75f);
                GUI.DrawTexture(
                    new Rect((Screen.width * 0.5f) - (CrosshairTexture.width * 0.5f),
                        (Screen.height * 0.5f) - (CrosshairTexture.height * 0.5f),
                        CrosshairTexture.width,
                        CrosshairTexture.height), CrosshairTexture);
                GUI.color = Color.white;
            }
        }
    }
}