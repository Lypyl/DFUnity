﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Imports Daggerfall sounds into Unity as AudioClip objects.
    /// Should only be attached to DaggerfallUnity (for which it is a required component).
    /// </summary>
    [RequireComponent(typeof(DaggerfallUnity))]
    public class SoundReader : MonoBehaviour
    {
        #region Fields

        DaggerfallUnity dfUnity;
        SndFile soundFile;

        Dictionary<int, AudioClip> clipDict2D = new Dictionary<int, AudioClip>();
        Dictionary<int, AudioClip> clipDict3D = new Dictionary<int, AudioClip>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets true if file reading is ready.
        /// </summary>
        public bool IsReady
        {
            get { return ReadyCheck(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets AudioClip based on Daggerfall sound index.
        /// Caches 2D and 3D sounds independently.
        /// </summary>
        /// <param name="soundIndex">Sound index.</param>
        /// <param name="_3D">True for a 3D sound, otherwise sound is 2D.</param>
        /// <returns>AudioClip or null.</returns>
        public AudioClip GetAudioClip(int soundIndex, bool _3D = true)
        {
            const float divisor = 1.0f / 128.0f;

            if (!ReadyCheck())
                return null;

            // Look for clip in cache
            AudioClip cachedClip = GetCachedClip(soundIndex, _3D);
            if (cachedClip)
                return cachedClip;

            // Get sound data
            DFSound dfSound;
            if (!soundFile.GetSound(soundIndex, out dfSound))
                return null;

            // Create audio clip
            AudioClip clip;
            string name = string.Format("DaggerfallClip [Index={0}, ID={1}]", soundIndex, (int)soundFile.BsaFile.GetRecordId(soundIndex));
#if UNITY_5_0
			clip = AudioClip.Create (name, dfSound.WaveData.Length, 1, SndFile.SampleRate, false);
            // TODO: Set AudioSource.spatialBlend property where appropriate.

#else
            clip = AudioClip.Create(name, dfSound.WaveData.Length, 1, SndFile.SampleRate, _3D, false);
#endif

            // Create data array
            float[] data = new float[dfSound.WaveData.Length];
            for (int i = 0; i < dfSound.WaveData.Length; i++)
                data[i] = (dfSound.WaveData[i] - 128) * divisor;

            // Set clip data
            clip.SetData(data, 0);

            // Cache the clip
            CacheClip(soundIndex, _3D, clip);

            return clip;
        }

        /// <summary>
        /// Gets AudioClip based on Daggerfall SoundID.
        /// </summary>
        /// <param name="soundID">Sound ID.</param>
        /// <param name="_3D">True for a 3D sound, otherwise sound is 2D.</param>
        /// <returns>AudioClip or null.</returns>
        public AudioClip GetAudioClip(uint soundID, bool _3D = true)
        {
            if (!ReadyCheck())
                return null;

            return GetAudioClip(soundFile.GetRecordIndex(soundID));
        }

        /// <summary>
        /// Gets AudioClip based on Daggerfall SoundClip enum.
        /// </summary>
        /// <param name="soundClip">SoundClip enum.</param>
        /// <param name="_3D">True for a 3D sound, otherwise sound is 2D.</param>
        /// <returns>AudioClip or null.</returns>
        public AudioClip GetAudioClip(SoundClips soundClip, bool _3D = true)
        {
            if (!ReadyCheck())
                return null;

            return GetAudioClip((int)soundClip);
        }

        /// <summary>
        /// Gets sound ID from index.
        /// </summary>
        /// <param name="soundIndex">Sound index.</param>
        /// <returns>Sound ID.</returns>
        public uint GetSoundID(int soundIndex)
        {
            if (!ReadyCheck())
                return 0;

            return soundFile.BsaFile.GetRecordId(soundIndex);
        }

        /// <summary>
        /// Gets sound index from ID.
        /// </summary>
        /// <param name="soundID">Sound ID.</param>
        /// <returns>Sound index.</returns>
        public int GetSoundIndex(uint soundID)
        {
            if (!ReadyCheck())
                return -1;

            return soundFile.GetRecordIndex((uint)soundID);
        }

        #endregion

        #region Private Methods

        private AudioClip GetCachedClip(int key, bool _3D)
        {
            if (_3D && clipDict3D.ContainsKey(key))
                return clipDict3D[key];
            else if (!_3D && clipDict2D.ContainsKey(key))
                return clipDict2D[key];
            else
                return null;
        }

        private void CacheClip(int key, bool _3D, AudioClip clip)
        {
            if (_3D)
                clipDict3D.Add(key, clip);
            else
                clipDict2D.Add(key, clip);
        }

        private bool ReadyCheck()
        {
            // Ensure we have a DaggerfallUnity reference
            if (dfUnity == null)
            {
                dfUnity = GetComponent<DaggerfallUnity>();
                if (!dfUnity)
                {
                    DaggerfallUnity.LogMessage("SoundReader: Could not get DaggerfallUnity component.");
                    return false;
                }
            }

            // Do nothing if DaggerfallUnity not ready
            if (!dfUnity.IsReady)
            {
                DaggerfallUnity.LogMessage("SoundReader: DaggerfallUnity component is not ready. Have you set your Arena2 path?");
                return false;
            }

            // Ensure sound reader is ready
            if (soundFile == null)
            {
                soundFile = new SndFile(Path.Combine(dfUnity.Arena2Path, SndFile.Filename), FileUsage.UseMemory, true);
            }

            return true;
        }

        #endregion
    }
}