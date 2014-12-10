﻿using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Example enemy motor.
    /// </summary>
    [RequireComponent(typeof(EnemySenses))]
    [RequireComponent(typeof(EnemyAttack))]
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyDeath))]
    [RequireComponent(typeof(EnemyBlood))]
    [RequireComponent(typeof(EnemySounds))]
    [RequireComponent(typeof(CharacterController))]
    public class EnemyMotor : MonoBehaviour
    {
        public float MoveSpeed = 5f;                // Speed enemy can move towards target using SimpleMove()
        public float FlySpeed = 5f;                 // Speed enemy can fly towards target using Move()
        public float OpenDoorDistance = 2f;         // Maximum distance to open door
        public float GiveUpTime = 4f;               // Time in seconds enemy will give up if target is unreachable

        EnemySenses senses;
        Vector3 targetPos;
        CharacterController controller;
        DaggerfallMobileUnit mobile;

        float stopDistance = 1.7f;                  // Used to prevent orbiting
        Vector3 lastTargetPos;                      // Target from previous update
        float giveUpTimer;                          // Timer before enemy gives up

        void Start()
        {
            senses = GetComponent<EnemySenses>();
            controller = GetComponent<CharacterController>();
            mobile = GetComponentInChildren<DaggerfallMobileUnit>();
        }

        void Update()
        {
            Move();
            OpenDoors();
        }

        #region Private Methods

        private void Move()
        {
            // Do nothing if playing a one-shot animation
            if (mobile.IsPlayingOneShot())
                return;

            // Remain idle when player not acquired
            if (senses.LastKnownPlayerPos == EnemySenses.ResetPlayerPos)
            {
                mobile.ChangeEnemyState(MobileStates.Idle);
                return;
            }

            // Enemy will keep moving towards last known player position
            targetPos = senses.LastKnownPlayerPos;
            if (targetPos == lastTargetPos)
            {
                // Increment countdown to giving up when target is uncreachable and player lost
                giveUpTimer += Time.deltaTime;
                if (giveUpTimer > GiveUpTime &&
                    !senses.PlayerInSight && !senses.PlayerInEarshot)
                {
                    // Target is unreachable or player lost for too long, time to give up
                    senses.LastKnownPlayerPos = EnemySenses.ResetPlayerPos;
                    return;
                }
            }
            else
            {
                // Still chasing, update last target and reset give up timer
                lastTargetPos = targetPos;
                giveUpTimer = 0;
            }

            // Get distance to target
            float distance = Vector3.Distance(targetPos, transform.position);

            // Flying enemies aim for player face
            if (mobile.Summary.Enemy.Behaviour == MobileBehaviour.Flying ||
                mobile.Summary.Enemy.Behaviour == MobileBehaviour.Spectral)
                targetPos.y += 0.9f;

            // Get direction and face target
            Vector3 direction = targetPos - transform.position;
            transform.forward = direction.normalized;

            // Move towards target
            if (distance > stopDistance)
            {
                mobile.ChangeEnemyState(MobileStates.Move);
                if (mobile.Summary.Enemy.Behaviour == MobileBehaviour.Flying ||
                    mobile.Summary.Enemy.Behaviour == MobileBehaviour.Spectral)
                    controller.Move(transform.forward * (FlySpeed * Time.deltaTime));
                else
                    controller.SimpleMove(transform.forward * ((MoveSpeed * 40f) * Time.deltaTime));    // Not sure why SimpleMove() needs to be scaled. Check this.
            }
            else
            {
                // We have reached target, is player nearby?
                if (!senses.PlayerInSight && !senses.PlayerInEarshot)
                    senses.LastKnownPlayerPos = EnemySenses.ResetPlayerPos;
            }
        }

        private void OpenDoors()
        {
            // Can we open doors?
            if (mobile.Summary.Enemy.CanOpenDoors)
            {
                // Is there a door blocking path to player?
                if (senses.LastKnownDoor != null && senses.DistanceToDoor < OpenDoorDistance)
                {
                    // Is the door closed? Try to open it!
                    if (!senses.LastKnownDoor.IsOpen)
                        senses.LastKnownDoor.ToggleDoor();
                }
            }
        }

        #endregion
    }
}