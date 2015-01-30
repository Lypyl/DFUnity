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

        EnemyMotor motor;
        DaggerfallMobileUnit mobile;
        EnemyBlood blood;

        void Start()
        {
            motor = GetComponent<EnemyMotor>();
            mobile = GetComponentInChildren<DaggerfallMobileUnit>();
            blood = GetComponent<EnemyBlood>();
        }

        /// <summary>
        /// Enemy has been damaged.
        /// </summary>
        public void RemoveHealth(GameObject sendingPlayer, float amount, Vector3 hitPosition)
        {
            Health -= amount;
            if (Health < 0)
                SendMessage("Die");

            // Aggro this enemy
            // To enhance, use a script that "shouts" to other enemies in range and make them hostile to player also
            motor.MakeEnemyHostileToPlayer(sendingPlayer);

            if (mobile != null)
            {
                blood.ShowBloodSplash(mobile.Summary.Enemy.BloodIndex, hitPosition);
            }

            //Debug.Log(string.Format("Enemy health is {0}", Health));
        }
    }
}