using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Daggerfall.Game { 
    public class ScrollManager : PrefabStateMachine {
        public UIManager uiManager;

        // Use this for initialization
        void Start () {
        
        }
        
        // Update is called once per frame
        void Update () {
/*            if (Input.GetMouseButtonDown(0)) {
                smallScroll.SetActive(false);
                uiManager.SendMessage("uiElementClosed");
            }*/
            base.Update();
        }

        void OnGUI() {
        }

        public void displayScroll(string output) {
            // TODO: Create a large scroll and figure out when to use it, even for localized text!
            changeState(PrefabState.UI_SCROLL_SMALL);
            sendMessageToPrefab(new KeyValuePair<string,object>("displayText", output));
            uiManager.SendMessage("uiElementOpened");

/*            smallScrollText.text = output;
            smallScroll.SetActive(true);
            uiManager.SendMessage("uiElementOpened");*/
        }

        public void closeScroll() { 
            changeState(PrefabState.UI_OFF);
            uiManager.SendMessage("uiElementClosed");
        }
    }
}
