using UnityEngine;
using System.Collections;
using DaggerfallWorkshop;

namespace Daggerfall.Gameplay
{
    /// <summary>
    /// Example class to represent player health.
    /// </summary>
    [RequireComponent(typeof(ShowPlayerDamage))]
    public class PlayerHealth : MonoBehaviour
    {
        void Start()
        {
        }

        /// <summary>
        /// Player has been damaged.
        /// </summary>
        void RemoveHealth()
        {
            GetComponent<ShowPlayerDamage>().Flash();
        }

        /// <summary>
        /// Player has been damaged by a fall.
        /// </summary>
        /// <param name="fallDistance"></param>
        void ApplyPlayerFallDamage(float fallDistance)
        {
            GetComponent<ShowPlayerDamage>().Flash();
        }
    }
}