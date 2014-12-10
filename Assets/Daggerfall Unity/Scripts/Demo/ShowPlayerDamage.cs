﻿using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Demo
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