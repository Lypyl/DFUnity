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

        public Canvas MenuOptions1;
        public Canvas DevConsoleCanvas;
        public Canvas PlayerSheet;
        public Canvas DebugHUD;
        public Canvas UIScroll;
        public Canvas SmallScroll;
        public Camera playerCamera; // necessary to change PlayerMouseLook behavior
        List<Canvas> openableUIElements;

        // Use this for initialization
        void Start () {
            MenuOptions1.enabled = false;
            DevConsoleCanvas.enabled = false;
            PlayerSheet.enabled = false;
            UIScroll.enabled = true;
            //SmallScroll.enabled = false;
            DebugHUD.enabled = true;
            openableUIElements = new List<Canvas>();
            openableUIElements.Add(MenuOptions1);
            openableUIElements.Add(DevConsoleCanvas);
            openableUIElements.Add(PlayerSheet);
            //openableUIElements.Add(SmallScroll);
            updateUIState();
        }
        
        // Update is called once per frame
        void Update () {
            // This whole area will need game logic for what can be opened when
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                MenuOptions1.enabled = !MenuOptions1.enabled;
                //playerCamera.GetComponent<PlayerMouseLook>().enableMLook = !MenuOptions1.enabled;
                    updateUIState();
            } else if (Input.GetButtonDown("DevConsole")) { 
                // TODO: For the love of God, refactor this
                DevConsole devConsoleScript = DevConsoleCanvas.GetComponent<DevConsole>();
                if (!DevConsoleCanvas.enabled) { 
                    DevConsoleCanvas.enabled = true;
                    //devConsoleScript.inputField.enabled = true;
                    devConsoleScript.inputField.Select();
                } else {
                    DevConsoleCanvas.enabled = false;
                    //devConsoleScript.inputField.enabled = false;
                }
                updateUIState();
            } else if (Input.GetButtonDown("PlayerSheet")) { 
                PlayerSheet.enabled = !PlayerSheet.enabled;
                updateUIState();
            }
        }

        // TODO: Optimization: aggressive inlining? 
        void updateUIState() {
            _isUIOpen = false;
            foreach (Canvas canvas in openableUIElements) {
                if (canvas.enabled) {
                    _isUIOpen = true;
                    break;
                }
            }
        }

        void debugHUD_displayText(string completeContents) {
            DebugHUD.GetComponentInChildren<Text>().text = completeContents;
        }

        void devConsole_displayText(string line) {
            DevConsoleCanvas.GetComponent<DevConsole>().displayText(line);
        }

        void uiScroll_displayText(string output) {
            if (UIScroll) {
                ScrollManager sm = UIScroll.GetComponent<ScrollManager>();
                if (sm) { 
                    sm.displayScroll(output);
                }
            }
        }

        void uiElementOpened() {
            updateUIState();
        }

        void uiElementClosed() {
            updateUIState();
        }
    }
}
