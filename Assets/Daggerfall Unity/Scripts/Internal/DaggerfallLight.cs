using UnityEngine;
using System;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Attached to point lights to create animated effect.
    /// </summary>
    public class DaggerfallLight : MonoBehaviour
    {
        public DaggerfallBillboard ParentBillboard;

        public bool Animate = false;

        DaggerfallUnity dfUnity;
        bool lastCityLightsFlag;

        float Variance = 1.5f;              // Maximum amount radius can vary per cycle
        float Speed = 0.6f;                 // Speed radius will shrink or grow towards varied radius
        float FramesPerSecond = 16f;        // Number of times per second animation will tick

        float startRange;
        float targetRange;
        bool stepping;

        void Start()
        {
            if (light != null)
                StartCoroutine(AnimateLight());
        }

        void Update()
        {
            // Do nothing if not ready
            if (!ReadyCheck())
                return;

            // Handle automated light enable/disable
            if (dfUnity.Option_AutomateCityLights && light)
            {
                // Only change if day/night flag changes
                if (lastCityLightsFlag != dfUnity.WorldTime.CityLightsOn)
                {
                    // Set light
                    light.enabled = dfUnity.WorldTime.CityLightsOn;
                    lastCityLightsFlag = dfUnity.WorldTime.CityLightsOn;
                }
            }
        }

        #region Private Methods

        IEnumerator AnimateLight()
        {
            startRange = light.range;

            while (Animate)
            {
                if (stepping)
                {
                    if (targetRange <= light.range)
                    {
                        light.range -= Speed;
                        if (light.range <= targetRange)
                            stepping = false;
                    }
                    else
                    {
                        light.range += Speed;
                        if (light.range >= targetRange)
                            stepping = false;
                    }
                }
                else
                {
                    // Start a new cycle
                    targetRange = UnityEngine.Random.Range(startRange - Variance, startRange);
                    stepping = true;
                }

                yield return new WaitForSeconds(1f / FramesPerSecond);
            }

            light.range = startRange;
        }

        private bool ReadyCheck()
        {
            // Ensure we have a DaggerfallUnity reference
            if (dfUnity == null)
            {
                if (!DaggerfallUnity.FindDaggerfallUnity(out dfUnity))
                {
                    DaggerfallUnity.LogMessage("DaggerfallLight: Could not get DaggerfallUnity component.");
                    return false;
                }

                // Force first update to set lights
                lastCityLightsFlag = !dfUnity.WorldTime.CityLightsOn;
            }

            // Do nothing if DaggerfallUnity not ready
            if (!dfUnity.IsReady)
            {
                DaggerfallUnity.LogMessage("DaggerfallLight: DaggerfallUnity component is not ready. Have you set your Arena2 path?");
                return false;
            }

            // Get billboard component
            if (ParentBillboard == null)
                return false;

            return true;
        }

        #endregion
    }
}
