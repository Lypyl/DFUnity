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
    /// General purpose texture animation on a single material.
    /// </summary>
    public class AnimateTexture : MonoBehaviour
    {
        public Material TargetMaterial;
        public Texture[] TextureArray;
        public float FramesPerSecond = 12;

        bool restartAnims;

        void Start()
        {
            StartCoroutine(AnimateTextures());
        }

        void OnDisable()
        {
            restartAnims = true;
        }

        void OnEnable()
        {
            // Restart animation coroutine if not running
            if (restartAnims)
            {
                StartCoroutine(AnimateTextures());
                restartAnims = false;
            }
        }

        IEnumerator AnimateTextures()
        {
            int currentTexture = 0;
            while (true)
            {
                if (TextureArray != null && TargetMaterial != null)
                {
                    TargetMaterial.mainTexture = TextureArray[currentTexture++];
                    if (currentTexture >= TextureArray.Length)
                        currentTexture = 0;
                }

                yield return new WaitForSeconds(1f / FramesPerSecond);
            }
        }
    }
}