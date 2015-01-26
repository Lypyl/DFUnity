using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Plays different ambient effects at random intervals.
    /// </summary>
    [RequireComponent(typeof(DaggerfallAudioSource))]
    public class AmbientEffectsPlayer : MonoBehaviour
    {
        public int MinWaitTime = 4;             // Min wait time in seconds before next sound
        public int MaxWaitTime = 35;            // Max wait time in seconds before next sound
        public AmbientSoundPresets Presets;     // Ambient sound preset
        public bool PlayLightningEffect;        // Play a lightning effect where appropriate
        public DaggerfallSky SkyForLightning;   // Sky to receive lightning effect
        public Light LightForLightning;         // Light to receive lightning effect

        System.Random random;
        DaggerfallAudioSource dfAudioSource;
        SoundClips[] ambientSounds;
        AudioClip rainLoop;
        float waitTime;
        float waitCounter;
        AmbientSoundPresets lastPresets;

        public enum AmbientSoundPresets
        {
            None,                   // No ambient sounds
            Dungeon,                // Dungeon ambience
            Storm,                  // Storm ambience
        }

        void Start()
        {
            random = new System.Random(System.DateTime.Now.Millisecond);
            dfAudioSource = GetComponent<DaggerfallAudioSource>();
            dfAudioSource.Preset = AudioPresets.OnDemand;
            ApplyPresets();
            StartWaiting();
        }

        void OnEnable()
        {
            // Ensures storm loop is rebooted after exiting interior
            rainLoop = null;
        }

        void Update()
        {
            // Change sound presets
            if (Presets != lastPresets)
            {
                lastPresets = Presets;
                ApplyPresets();
                StartWaiting();
            }

            // Start storm loop if not running
            if (Presets == AmbientSoundPresets.Storm && rainLoop == null)
            {
                rainLoop = dfAudioSource.GetAudioClip((int)SoundClips.AmbientRaining, false);
                dfAudioSource.AudioSource.clip = rainLoop;
                dfAudioSource.AudioSource.loop = true;
                dfAudioSource.AudioSource.Play();
            }

            // Tick counter
            waitCounter += Time.deltaTime;
            if (waitCounter > waitTime)
            {
                PlayEffects();
                StartWaiting();
            }
        }

        #region Private Methods

        private void PlayEffects()
        {
            // Do nothing if audio not setup
            if (dfAudioSource == null || ambientSounds == null)
                return;

            // Get next sound index
            int index = random.Next(0, ambientSounds.Length);

            // Play effect
            if (Presets == AmbientSoundPresets.Storm && PlayLightningEffect)
            {
                // Play lightning effects together with appropriate sounds
                StartCoroutine(PlayLightningEffects(index));
            }
            else
            {
                // Play ambient sound as a one-shot 2D sound
                dfAudioSource.PlayOneShot((int)ambientSounds[index], false);
            }
        }

        private IEnumerator PlayLightningEffects(int index)
        {
            //Debug.Log(string.Format("Playing index {0}", index));

            int minFlashes = 5;
            int maxFlashes = 10;
            float soundDelay = 0f;
            float randomSkip = 0.6f;

            // Store starting values
            float startLightIntensity = 1f;
            float startSkyScale = 1f;
            if (SkyForLightning) startSkyScale = SkyForLightning.SkyColorScale;
            if (LightForLightning) startLightIntensity = LightForLightning.intensity;

            SoundClips clip = ambientSounds[index];
            if (clip == SoundClips.StormLightningShort)
            {
                // Short close lightning flash
                minFlashes = 4;
                maxFlashes = 8;
            }
            else if (clip == SoundClips.StormLightningThunder)
            {
                // Short close lightning flash followed by thunder
                minFlashes = 5;
                maxFlashes = 10;
            }
            else if (clip == SoundClips.StormThunderRoll)
            {
                // Distant lightning strike with followed by a long delay then rolling thunder
                minFlashes = 20;
                maxFlashes = 30;
                soundDelay = 1.7f;
            }
            else
            {
                // Unknown clip, just play as one-shot and exit
                dfAudioSource.PlayOneShot((int)clip, false);
                yield break;
            }

            // Play lightning flashes
            int numFlashes = random.Next(minFlashes, maxFlashes);
            for (int i = 0; i < numFlashes; i++)
            {
                // Randomly skip frames to introduce delay between flashes
                if (Random.value < randomSkip)
                {
                    // Flash on
                    if (SkyForLightning) SkyForLightning.SkyColorScale = 2f;
                    if (LightForLightning) LightForLightning.intensity = 2f;
                    yield return new WaitForEndOfFrame();
                }

                // Flash off
                if (SkyForLightning) SkyForLightning.SkyColorScale = startSkyScale;
                if (LightForLightning) LightForLightning.intensity = startLightIntensity;
                yield return new WaitForEndOfFrame();
            }

            // Reset values just to be sure
            if (SkyForLightning) SkyForLightning.SkyColorScale = startSkyScale;
            if (LightForLightning) LightForLightning.intensity = startLightIntensity;

            // Delay for sound effect
            if (soundDelay > 0)
                yield return new WaitForSeconds(1f / soundDelay);

            // Play sound effect
            dfAudioSource.PlayOneShot((int)clip, false);

            yield break;
        }

        private void StartWaiting()
        {
            // Reset countdown to next sound
            waitTime = random.Next(MinWaitTime, MaxWaitTime);
            waitCounter = 0;
        }

        private void ApplyPresets()
        {
            if (Presets == AmbientSoundPresets.Dungeon)
            {
                // Set dungeon one-shots
                ambientSounds = new SoundClips[] {
                    SoundClips.AmbientDripShort,
                    SoundClips.AmbientDripLong,
                    SoundClips.AmbientWindMoan,
                    SoundClips.AmbientWindMoanDeep,
                    SoundClips.AmbientDoorOpen,
                    SoundClips.AmbientGrind,
                    SoundClips.AmbientStrumming,
                    SoundClips.AmbientWindBlow1,
                    SoundClips.AmbientWindBlow2,
                    SoundClips.AmbientMetalJangleLow,
                    SoundClips.AmbientBirdCall,
                    SoundClips.AmbientSqueaks,
                    SoundClips.AmbientClank,
                    SoundClips.AmbientDistantMoan,
                };
            }
            else if (Presets == AmbientSoundPresets.Storm)
            {
                // Set storm one-shots
                ambientSounds = new SoundClips[] {
                    SoundClips.StormLightningShort,
                    SoundClips.StormLightningThunder,
                    SoundClips.StormThunderRoll,
                };
            }
            else
            {
                ambientSounds = null;
            }

            lastPresets = Presets;
            dfAudioSource.SetSound(0, AudioPresets.OnDemand, false);
        }

        #endregion
    }
}