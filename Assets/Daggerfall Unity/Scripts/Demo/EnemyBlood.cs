using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Demo
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
