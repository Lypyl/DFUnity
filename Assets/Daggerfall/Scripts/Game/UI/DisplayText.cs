using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Daggerfall.Game.UI { 
    public class DisplayText : MonoBehaviour {
        public Text text;

        void displayText(string output) { 
            if (text) { 
                text.text = output;
            }
        }

        // Use this for initialization
        void Start () {
        
        }
        
        // Update is called once per frame
        void Update () {
        
        }
    }
}
