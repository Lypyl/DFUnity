using UnityEngine;
using System.Collections;

namespace DaggerfallWorkshop.Game {

    public class DevConsole : MonoBehaviour {

        DaggerfallUnity dfUnity;
        float deltaTime = 0.0f;

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
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
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
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }
}