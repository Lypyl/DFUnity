using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Daggerfall.Game.UI { 
    public class DisplayText : MonoBehaviour {

        void displayText(string output) { 
            Text t = gameObject.GetComponent<Text>();
            if (t) { 
                t.text = output;
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
