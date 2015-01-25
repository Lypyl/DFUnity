// Project:         Daggerfall Unity -- A game built with Daggerfall Tools For Unity
// Description:     This is a modified version of a script provided by Daggerfall Tools for Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Project Page:    https://github.com/EBFEh/DFUnity -- https://code.google.com/p/daggerfall-unity/

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;

namespace Daggerfall.Gameplay
{
    /// <summary>
    /// Example enemy blood effect.
    /// </summary>
    public class EnemyBlood : MonoBehaviour
    {
        const int bloodArchive = 380;

        public void ShowBloodSplash(int bloodIndex, Vector3 bloodPosition)
        {
            // Create oneshot animated billboard for blood effect
            DaggerfallUnity dfUnity;
            if (DaggerfallUnity.FindDaggerfallUnity(out dfUnity))
            {
                GameObject go = GameObjectHelper.CreateDaggerfallBillboardGameObject(dfUnity, bloodArchive, bloodIndex, null, true);
                go.name = "BloodSplash";
                DaggerfallBillboard c = go.GetComponent<DaggerfallBillboard>();
                go.transform.position = bloodPosition + transform.forward * 0.02f;
                c.OneShot = true;
                c.FramesPerSecond = 10;
            }
        }
    }
}
