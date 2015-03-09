using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Demo;
using DaggerfallWorkshop;
using UnityEngine.UI;
using Daggerfall.Game;
using System.Collections.Generic;

namespace Daggerfall.Game { 
    public class UIManager : MonoBehaviour {
        private bool _isUIOpen = false;
        public bool isUIOpen { 
            get { return _isUIOpen; }
        }

        // TODO: Refactor with PFSM
        public GameObject MenuOptions1;
        public GameObject DevConsole;
        public GameObject PlayerSheet;
        public GameObject DebugHUD;
        public PrefabStateMachine UIScroll;

        public ScrollManager scrollManager;
        public Camera playerCamera; // necessary to change PlayerMouseLook behavior
        List<GameObject> openableUIElements;
        List<PrefabStateMachine> PFSMs;

        // Use this for initialization
        void Start () {
            MenuOptions1.SetActive(false);
            DevConsole.SetActive(false);
            PlayerSheet.SetActive(false);
            DebugHUD.SetActive(true);
            openableUIElements = new List<GameObject>();
            openableUIElements.Add(MenuOptions1);
            openableUIElements.Add(DevConsole);
            openableUIElements.Add(PlayerSheet);

            PFSMs = new List<PrefabStateMachine>();
            PFSMs.Add(UIScroll);

            updateUIState();
        }
        
        // Update is called once per frame
        void Update () {
            // This whole area will need game logic for what can be opened when
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                //MenuOptions1.activeSelf = !MenuOptions1.active;
                //playerCamera.GetComponent<PlayerMouseLook>().enableMLook = !MenuOptions1.enabled;
                updateUIState();
            } else if (Input.GetButtonDown("DevConsole")) { 
                DevConsole devConsoleScript = DevConsole.GetComponent<DevConsole>();
                if (!DevConsole.activeSelf) { 
                    DevConsole.SetActive(true);
                    //devConsoleScript.inputField.enabled = true;
                    devConsoleScript.inputField.Select();
                } else {
                    DevConsole.SetActive(false);
                    //devConsoleScript.inputField.enabled = false;
                }
                updateUIState();
            } else if (Input.GetButtonDown("PlayerSheet")) { 
                //PlayerSheet.enabled = !PlayerSheet.enabled;
                updateUIState();
            }
        }

        // TODO: Optimization: aggressive inlining? 
        void updateUIState() {
            _isUIOpen = false;
            foreach (GameObject go in openableUIElements) {
                if (go.activeSelf) {
                    _isUIOpen = true;
                    break;
                }
            }

            // TODO: Convert to just one system
            foreach (PrefabStateMachine PFSM in PFSMs) { 
                if (PFSM.currentOrPendingState != PrefabState.UI_OFF) { 
                    _isUIOpen = true;
                    break;
                }
            }
        }

        void debugHUD_displayText(string completeContents) {
            DebugHUD.GetComponentInChildren<Text>().text = completeContents;
        }

        void devConsole_displayText(string line) {
            DevConsole.GetComponent<DevConsole>().displayText(line);
        }

        void uiScroll_displayText(string output) {
                scrollManager.displayScroll(output);
        }

        void uiElementOpened() {
            updateUIState();
        }

        void uiElementClosed() {
            updateUIState();
        }
    }
}
