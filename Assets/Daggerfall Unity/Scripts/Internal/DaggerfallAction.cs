﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Defines and executes Daggerfall action records.
    /// </summary>
    public class DaggerfallAction : MonoBehaviour
    {
        public bool ActionEnabled = false;                          // Enable/disable action
        public bool CanReverse = true;                              // If true action can be played forward and backward
        public bool PlaySound = true;                               // Play sound if present (ActionSound > 0)
        public string ModelDescription = string.Empty;              // Description string for this model
        public int ActionFlags = 0;                                 // Action flag value
        public int ActionSoundID = 0;                                 // Action sound ID
        public Vector3 ActionRotation = Vector3.zero;               // Rotation to perform
        public Vector3 ActionTranslation = Vector3.zero;            // Translation to perform
        public float ActionDuration = 0;                            // Time to reach final state
        public GameObject NextObject;                               // Next object in action chain
        public GameObject PreviousObject;                           // Previous object in action chain

        ActionState currentState;
        Vector3 startPosition;

        public enum ActionState
        {
            Start,
            Playing,
            End,
        }

        ActionState CurrentState
        {
            get { return currentState; }
            set { SetState(value); }
        }

        void Start()
        {
            currentState = ActionState.Start;
            startPosition = transform.position;

            // Post-fix "LID" action for unrotated coffin lids in Scourg Barrow.
            // The game data seems to be wrong here.
            if (ModelDescription == "LID" && transform.eulerAngles.y == 0)
                ActionRotation.z = 90f;
        }

        void Update()
        {
        }

        public void Play(bool playNext = true)
        {
            // Do nothing if action disabled or in motion
            if (!ActionEnabled || IsPlaying())
                return;

            // Start playing action
            switch(currentState)
            {
                case ActionState.Start:
                    TweenToEnd();
                    break;
                case ActionState.End:
                    if (CanReverse) TweenToStart();
                    break;
            }
            currentState = ActionState.Playing;

            // Start playing sound if flagged and ready
            if (PlaySound && ActionSoundID > 0 && audio)
            {
                audio.Play();
            }

            // Play next action in chain
            if (NextObject != null && playNext)
                NextObject.GetComponent<DaggerfallAction>().Play();
        }

        public bool IsPlaying()
        {
            // Check if this action or any chained action is playing
            if (currentState == ActionState.Playing)
            {
                return true;
            }
            else
            {
                if (NextObject == null)
                    return false;

                DaggerfallAction nextAction = NextObject.GetComponent<DaggerfallAction>();
                if (nextAction == null)
                    return false;

                return nextAction.IsPlaying();
            }
        }

        public void SetState(ActionState state)
        {
            currentState = state;
        }

        #region Private Methods

        private void TweenToEnd()
        {
            Hashtable rotateParams = __ExternalAssets.iTween.Hash(
                "amount", new Vector3(ActionRotation.x / 360f, ActionRotation.y / 360f, ActionRotation.z / 360f),
                "space", Space.World,
                "time", ActionDuration,
                "easetype", __ExternalAssets.iTween.EaseType.linear);

            Hashtable moveParams = __ExternalAssets.iTween.Hash(
                "position", startPosition + ActionTranslation,
                "time", ActionDuration,
                "easetype", __ExternalAssets.iTween.EaseType.linear,
                "oncomplete", "SetState",
                "oncompleteparams", ActionState.End);

            __ExternalAssets.iTween.RotateBy(gameObject, rotateParams);
            __ExternalAssets.iTween.MoveTo(gameObject, moveParams);
        }

        private void TweenToStart()
        {
            Hashtable rotateParams = __ExternalAssets.iTween.Hash(
                "amount", new Vector3(-ActionRotation.x / 360f, -ActionRotation.y / 360f, -ActionRotation.z / 360f),
                "space", Space.World,
                "time", ActionDuration,
                "easetype", __ExternalAssets.iTween.EaseType.linear);

            Hashtable moveParams = __ExternalAssets.iTween.Hash(
                "position", startPosition,
                "time", ActionDuration,
                "easetype", __ExternalAssets.iTween.EaseType.linear,
                "oncomplete", "SetState",
                "oncompleteparams", ActionState.Start);

            __ExternalAssets.iTween.RotateBy(gameObject, rotateParams);
            __ExternalAssets.iTween.MoveTo(gameObject, moveParams);
        }

        #endregion
    }
}