﻿// Project:         Daggerfall Unity -- A game built with Daggerfall Tools For Unity
// Description:     This is a modified version of a script provided by Daggerfall Tools for Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Project Page:    https://github.com/EBFEh/DFUnity -- https://code.google.com/p/daggerfall-unity/

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop;

namespace Daggerfall.Gameplay
{
    /// <summary>
    /// Simple component to flash screen red on player damage.
    /// </summary>
    public class ShowPlayerDamage : MonoBehaviour
    {
        Texture2D damageTexture;
        bool fadingOut = false;
        float alphaFadeValue = 0;
        float fadeSpeed = 0.7f;

        void Start()
        {
            if (damageTexture == null)
                damageTexture = __ExternalAssets.iTween.CameraTexture(new Color(1, 0, 0, 1));
        }

        public void Flash()
        {
            alphaFadeValue = 0.4f;
            fadingOut = true;
        }

        void OnGUI()
        {
            if (fadingOut)
            {
                alphaFadeValue -= fadeSpeed * Time.deltaTime;
                if (alphaFadeValue > 0)
                {
                    GUI.color = new Color(1, 0, 0, alphaFadeValue);
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), damageTexture);
                }
                else
                {
                    alphaFadeValue = 0;
                    fadingOut = false;
                }
            }
        }
    }
}