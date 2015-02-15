﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2015 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    https://github.com/Interkarma/daggerfall-unity

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using AudioSynthesis.Bank;
using AudioSynthesis.Sequencer;
using AudioSynthesis.Synthesis;
using AudioSynthesis.Midi;

namespace DaggerfallWorkshop
{
    [RequireComponent(typeof(AudioSource))]
    public class DaggerfallSongPlayer : MonoBehaviour
    {
        const int sampleRate = 44100;
        const int polyphony = 100;

        [NonSerialized, HideInInspector]
        public bool IsPlaying = false;
        [NonSerialized, HideInInspector]
        public int CurrentTime = 0;
        [NonSerialized, HideInInspector]
        public int EndTime = 0;

        public bool ShowDebugString = false;

        [Range(0.0f, 10.0f)]
        public float Gain = 5.0f;
        public string SoundBank = "chorium.sf2";
        public string SongFolder = "Songs/";
        public SongFiles Song = SongFiles.song_none;

        AudioSource audioSource;
        Synthesizer midiSynthesizer = null;
        MidiFileSequencer midiSequencer = null;
        string currentMidiName;
        float[] sampleBuffer = new float[0];
        int channels = 0;
        int bufferLength = 0;
        int numBuffers = 0;
        bool playEnabled = false;
        bool awakeComplete = false;

        /// <summary>
        /// Gets peer AudioSource component.
        /// </summary>
        public AudioSource AudioSource
        {
            get { return (audioSource) ? audioSource : GetComponent<AudioSource>(); }
        }

        void Start()
        {
            InitSynth();
            audioSource.enabled = false;
        }

        void Update()
        {
            // Update status
            if (midiSequencer != null)
            {
                IsPlaying = midiSequencer.IsPlaying;
                CurrentTime = midiSequencer.CurrentTime;
                EndTime = midiSequencer.EndTime;
            }
        }

        void LateUpdate()
        {
            if (audioSource.playOnAwake && !midiSequencer.IsPlaying && !awakeComplete)
            {
                Play();
                awakeComplete = true;
            }
            if (audioSource.loop && !midiSequencer.IsPlaying)
            {
                Play();
            }
        }

        void OnGUI()
        {
            if (Event.current.type.Equals(EventType.Repaint) && ShowDebugString)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.black;
                string text = GetDebugString();
                GUI.Label(new Rect(10, 50, 800, 24), text, style);
                GUI.Label(new Rect(8, 48, 800, 24), text);
            }
        }

        public void Play()
        {
            if (Song == SongFiles.song_none)
                return;

            Play(Song);
        }

        /// <summary>
        /// Play current song.
        /// </summary>
        public void Play(SongFiles song)
        {
            if (!InitSynth())
                return;

            // Stop if playing another song
            Stop();

            // Ensure audio source is enabled
            audioSource.enabled = true;

            // Load song data
            string filename = EnumToFilename(song);
            byte[] songData = LoadSong(filename);
            if (songData == null)
                return;

            // Create song
            MidiFile midiFile = new MidiFile(new MyMemoryFile(songData, filename));
            if (midiSequencer.LoadMidi(midiFile))
            {
                midiSequencer.Play();
                currentMidiName = filename;
                playEnabled = true;
            }       
        }

        /// <summary>
        /// Stop playing song.
        /// </summary>
        public void Stop()
        {
            if (!InitSynth())
                return;

            // Disable audio source
            // This is done to reduce number of active audio sources in scene
            // If there are too many audio sources then playback will stall
            // This is common in dungeons which have many audio sources
            audioSource.enabled = false;

            // Stop if playing a song
            if (midiSequencer.IsPlaying)
            {
                midiSequencer.Stop();
                midiSynthesizer.NoteOffAll(true);
                midiSynthesizer.ResetSynthControls();
                midiSynthesizer.ResetPrograms();
                playEnabled = false;
            }
        }

        #region Private Methods

        private bool InitSynth()
        {
            // Get peer AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                DaggerfallUnity.LogMessage("DaggerfallSongPlayer: Could not find AudioSource component.");
                return false;
            }

            // Create synthesizer and load bank
            if (midiSynthesizer == null)
            {
                // Get number of channels
                if (AudioSettings.driverCaps.ToString() == "Stereo")
                    channels = 2;
                else
                    channels = 1;

                // Create synth
                AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
                midiSynthesizer = new Synthesizer(sampleRate, channels, bufferLength / numBuffers, numBuffers, polyphony);

                // Load bank data
                byte[] bankData = LoadBank(SoundBank);
                if (bankData == null)
                    return false;
                else
                {
                    midiSynthesizer.LoadBank(new MyMemoryFile(bankData, SoundBank));
                    midiSynthesizer.ResetSynthControls(); // Need to do this for bank to load properly, don't know why
                }
            }

            // Create sequencer
            if (midiSequencer == null)
                midiSequencer = new MidiFileSequencer(midiSynthesizer);

            // Check init
            if (midiSynthesizer == null || midiSequencer == null)
            {
                DaggerfallUnity.LogMessage("DaggerfallSongPlayer: Failed to init synth.");
                return false;
            }

            return true;
        }

        private string EnumToFilename(SongFiles song)
        {
            string enumName = song.ToString();
            return enumName.Remove(0, "song_".Length) + ".mid";
        }

        private byte[] LoadBank(string filename)
        {
            TextAsset asset = Resources.Load<TextAsset>(filename);
            if (asset != null)
            {
                return asset.bytes;
            }

            DaggerfallUnity.LogMessage(string.Format("DaggerfallSongPlayer: Bank file '{0}' not found.", filename));

            return null;
        }

        private byte[] LoadSong(string filename)
        {
            TextAsset asset = Resources.Load<TextAsset>(Path.Combine(SongFolder, filename));
            if (asset != null)
            {
                return asset.bytes;
            }

            DaggerfallUnity.LogMessage(string.Format("DaggerfallSongPlayer: Song file '{0}' not found.", filename));

            return null;
        }

        private string GetDebugString()
        {
            if (midiSequencer == null)
                return "Sequencer not ready.";
            if (midiSynthesizer == null)
                return "Synthesizer not ready.";

            string final;
            if (midiSequencer.IsPlaying)
                final = string.Format("Playing song '{0}' at position {1}/{2}", currentMidiName, midiSequencer.CurrentTime, midiSequencer.EndTime);
            else
                final = string.Format("Song '{0}' ready. Not playing.", currentMidiName);

            return final;
        }

        #endregion

        #region Audio Filter

        // Called when audio filter needs more sound data
        void OnAudioFilterRead(float[] data, int channels)
        {
            // Do nothing if play not enabled
            // This flag is raised/lowered when user starts/stops play
            // Helps avoids thread finding synth in state of shutting down
            if (!playEnabled)
                return;

            // Must have synth and seq
            if (midiSynthesizer == null || midiSequencer == null)
                return;

            // Sample buffer size must match working buffer size
            if (sampleBuffer.Length != midiSynthesizer.WorkingBufferSize)
                sampleBuffer = new float[midiSynthesizer.WorkingBufferSize];

            try
            {
                // Update sequencing - must be playing a song
                if (midiSequencer.IsMidiLoaded && midiSequencer.IsPlaying)
                {
                    midiSequencer.FillMidiEventQueue();
                    midiSynthesizer.GetNext(sampleBuffer);
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = sampleBuffer[i] * Gain;
                    }
                }
            }
            catch (Exception)
            {
                // Will sometimes drop here if Unity tries to feed audio filter
                // from another thread while synth is starting up or shutting down
                // Just nom the exception
            }
        }

        #endregion

        #region Interface Implementation

        public class MyMemoryFile : AudioSynthesis.IResource
        {
            private byte[] file;
            private string fileName;
            public MyMemoryFile(byte[] file, string fileName)
            {
                this.file = file;
                this.fileName = fileName;
            }
            public string GetName() { return fileName; }
            public bool DeleteAllowed() { return false; }
            public bool ReadAllowed() { return true; }
            public bool WriteAllowed() { return false; }
            public void DeleteResource() { return; }
            public Stream OpenResourceForRead() { return new MemoryStream(file); }
            public Stream OpenResourceForWrite() { return null; }
        }

        #endregion

        #region Static Song Arrays

        /// <summary>
        /// Just the GM songs.
        /// </summary>
        public static SongFiles[] Songs_GM = new SongFiles[]
        {
            SongFiles.song_02,
            SongFiles.song_03,
            SongFiles.song_04,
            SongFiles.song_05,
            SongFiles.song_06,
            SongFiles.song_07,
            SongFiles.song_08,
            SongFiles.song_09,
            SongFiles.song_10,
            SongFiles.song_11,
            SongFiles.song_12,
            SongFiles.song_13,
            SongFiles.song_15,
            SongFiles.song_16,
            SongFiles.song_17,
            SongFiles.song_18,
            SongFiles.song_20,
            SongFiles.song_21,
            SongFiles.song_22,
            SongFiles.song_23,
            SongFiles.song_25,
            SongFiles.song_28,
            SongFiles.song_29,
            SongFiles.song_30,
            SongFiles.song_d1,
            SongFiles.song_d10,
            SongFiles.song_d2,
            SongFiles.song_d3,
            SongFiles.song_d4,
            SongFiles.song_d5,
            SongFiles.song_d6,
            SongFiles.song_d7,
            SongFiles.song_d8,
            SongFiles.song_d9,
            SongFiles.song_dungeon,
            SongFiles.song_dungeon5,
            SongFiles.song_dungeon6,
            SongFiles.song_dungeon7,
            SongFiles.song_dungeon8,
            SongFiles.song_dungeon9,
            SongFiles.song_folk1,
            SongFiles.song_folk2,
            SongFiles.song_folk3,
            SongFiles.song_gbad,
            SongFiles.song_gcurse,
            SongFiles.song_gday___d,
            SongFiles.song_gdngn10,
            SongFiles.song_gdngn11,
            SongFiles.song_gdungn4,
            SongFiles.song_gdungn9,
            SongFiles.song_geerie,
            SongFiles.song_ggood,
            SongFiles.song_gmage_3,
            SongFiles.song_gneut,
            SongFiles.song_gpalac,
            SongFiles.song_gruins,
            SongFiles.song_gshop,
            SongFiles.song_gsneak2,
            SongFiles.song_gsnow__b,
            SongFiles.song_gsunny2,
            SongFiles.song_magic_2,
            SongFiles.song_overcast,
            SongFiles.song_overlong,
            SongFiles.song_oversnow,
            SongFiles.song_raining,
            SongFiles.song_sneaking,
            SongFiles.song_sneakng2,
            SongFiles.song_snowing,
            SongFiles.song_square_2,
            SongFiles.song_sunnyday,
            SongFiles.song_swimming,
            SongFiles.song_tavern,
        };

        #endregion
    }
}