using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Daggerfall.Game {
    public enum PrefabState { 
        UI_OFF,
        UI_SCROLL_SMALL,
        UI_SCROLL_LARGE,
        UI_SCROLL_HUGE,
        UI_SCROLL_DIALOG_YN,
        UI_SCROLL_DIALOG_YNC
    };

    [Serializable]
    public struct Prefab { 
        public PrefabState state;
        public GameObject prefabObject;
    }

    public class PrefabStateMachine : MonoBehaviour {
        public Prefab[] prefabs;
        private GameObject currentPrefab = null;
        private PrefabState _state = PrefabState.UI_OFF;
        private PrefabState _requestedState = PrefabState.UI_OFF;
        private List<KeyValuePair<string, object>> _queuedMessages = new List<KeyValuePair<string, object>>();

        public PrefabState currentState { 
            get { return _state; } 
        }

        /**
         * currentState will always reflect the state at that instant in time,
         * but sometimes you want to know is the state machine is about to switch to another state
         * (and Update() hasn't had a chance to run yet).
         **/
        public PrefabState currentOrPendingState { 
            get { return _requestedState; }
        }

        /**
         * Destroys the current prefab
         * Sets the state to the new state
         * Finds the proper prefab for the state and instantiates it
         **/
        protected virtual void Update() { 
            if (_requestedState == _state) return;
            _state = _requestedState;

            //TODO: Send out End() update?

            if (currentPrefab) { 
                Destroy(currentPrefab);
            }

            foreach (Prefab prefab in prefabs) { 
                if (prefab.state == _state) { 
                    currentPrefab = Instantiate(prefab.prefabObject);
                    break;
                }
            }

            // Dispatch any messages intended for this guy
            foreach (KeyValuePair<string, object> kvp in _queuedMessages) { 
                currentPrefab.SendMessage(kvp.Key, kvp.Value);
            }
        }

        protected virtual void sendMessageToPrefab(KeyValuePair<string, object> methodObjectValuePair) {
            _queuedMessages.Add(methodObjectValuePair);
        }

        public void changeState(PrefabState state) { 
            _requestedState = state;
        }

        public void disableUI() {
            changeState(PrefabState.UI_OFF);
        }

    }
}
