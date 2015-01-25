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