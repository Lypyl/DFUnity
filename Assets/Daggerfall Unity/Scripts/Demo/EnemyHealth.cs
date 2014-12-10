using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Example enemy health.
    /// </summary>
    [RequireComponent(typeof(EnemyBlood))]
    public class EnemyHealth : MonoBehaviour
    {
        public float Health = 100f;

        DaggerfallMobileUnit mobile;
        EnemyBlood blood;

        void Start()
        {
            mobile = GetComponentInChildren<DaggerfallMobileUnit>();
            blood = GetComponent<EnemyBlood>();
        }

        /// <summary>
        /// Enemy has been damaged.
        /// </summary>
        public void RemoveHealth(float amount, Vector3 hitPosition)
        {
            Health -= amount;
            if (Health < 0)
                SendMessage("Die");

            if (mobile != null)
            {
                blood.ShowBloodSplash(mobile.Summary.Enemy.BloodIndex, hitPosition);
            }

            //Debug.Log(string.Format("Enemy health is {0}", Health));
        }
    }
}