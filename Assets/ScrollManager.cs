using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Daggerfall.Game { 
    public class ScrollManager : MonoBehaviour {
        public Canvas smallScroll;
        public Text smallScrollText;
        public UIManager uiManager;

        // Use this for initialization
        void Start () {
        
        }
        
        // Update is called once per frame
        void Update () {
            if (Input.GetMouseButtonDown(0)) {
                this.smallScroll.enabled = false;
                uiManager.SendMessage("uiElementClosed");
            }
        }

        void OnGUI() {
        }

        public void displayScroll(string output) {
            // TODO: Create a large scroll and figure out when to use it, even for localized text!
            smallScrollText.text = output;
            smallScroll.enabled = true;
            uiManager.SendMessage("uiElementOpened");
        }
    }
}
