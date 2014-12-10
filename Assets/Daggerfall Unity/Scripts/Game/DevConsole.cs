using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Game {

    public class DevConsole : MonoBehaviour {

        DaggerfallUnity dfUnity;

        // Use this for initialization
        void Start () {
        
        }

        // Sanity check
        void Check() {
            if (!dfUnity) { 
                if(!DaggerfallUnity.FindDaggerfallUnity(out dfUnity)) { 
                    DaggerfallUnity.LogMessage(Error.ERROR_GAME_DAG_UNITY_NOT_FOUND);
                    Application.Quit();
                }
            }
        }

        // Update is called once per frame
        void Update () {
            Check();
            if (dfUnity.IsReady) {
                if (Input.GetButtonDown("DevConsole")) {
                    if (dfUnity.devConsoleOpen) {
                        dfUnity.devConsoleOpen = false;
                    } else {
                        dfUnity.devConsoleOpen = true;
                    }
                }
            } 
        }

        void OnGUI() {
            Check();

            if (dfUnity.IsReady) {
                if (dfUnity.devConsoleOpen) {
                    drawDevConsole();
                }
            }
        }

        void drawDevConsole() {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Dev Console");
        }
    }
}