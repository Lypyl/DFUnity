using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Demo;
using Daggerfall.Game.Quest;
using System.Xml;

namespace Daggerfall.Game {

    // Displays internal game messages and allows the user to execute internal game commands
    // Call displayText(...) to append a message to the log
    public class DevConsole : MonoBehaviour {
        public Text outputTextField;
        public InputField inputField;
        public UIManager uiManager;
        public StreamingWorld streamingWorldOwner;
        public WeatherManager weatherManager;
             
        DaggerfallUnity dfUnity;
        float deltaTime = 0.0f;
        static string _outputText = "";

        private const string TRAVEL_CMD = "travel";
        private const string SPAWN_ENEMY_CMD = "spawn_enemy";
        private const string SNOW_COMMAND = "snow";
        private const string XML_DEBUG = "xml";
        private const string QUEST_DEBUG = "quest";
        private const string TIME_DEBUG = "time";

        public void displayText(string text, bool newline = true) {
            // TODO: can string.concat ever buffer overflow?
            _outputText += System.DateTime.UtcNow + " *** " + text;
            if (newline) { 
                _outputText += "\n";
            }
            outputTextField.text = _outputText;
        }

        public static void flushText() {
            _outputText = "";
        }

        // Use this for initialization
        void Start () {
            displayText("Dev console created");
            enabled = false;
            Check();
        }

        // Sanity check
        void Check() {
            if (!dfUnity) {
                dfUnity = DaggerfallUnity.Instance;
            }
        }

        // Update is called once per frame
        void Update () {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        }

        void OnGUI() {
            if (!uiManager.isUIOpen) return;
            //inputField.enabled = true;
            drawFPS();
        }

        bool parseCommand(string[] args) {
            bool handled = false;
            Logger l = Logger.GetInstance();
            
            switch(args[0]) { 
                case (SPAWN_ENEMY_CMD):
                    if (args.Length == 2) {
                        int mobileType = System.Convert.ToInt32(args[1]);
                        if (mobileType > -1 && mobileType < 147) {
                            l.log("Spawning enemy of type " + (MobileTypes)mobileType + ".\n");
                            GameObject.FindGameObjectWithTag("EnemySpawner").SendMessage("SpawnEnemy", mobileType); 
                            handled = true;
                        }
                    }
                    break;
                case (TRAVEL_CMD):
                    if (args.Length >= 2) {
                        DFLocation location;
                        string nameWithPossibleSpaces = string.Join(" ", args);
                        nameWithPossibleSpaces = nameWithPossibleSpaces.Substring(TRAVEL_CMD.Length + 1);
                        if (!GameObjectHelper.FindMultiNameLocation(nameWithPossibleSpaces, out location)) {
                            l.log("Unable to find location " + nameWithPossibleSpaces + ".\n");
                        } else {
                            l.log("Found location in " + location.RegionName + "!\n");
                            DFPosition mapPos = MapsFile.LongitudeLatitudeToMapPixel((int)location.MapTableData.Longitude, (int)location.MapTableData.Latitude);
                            if (mapPos.X >= TerrainHelper.minMapPixelX || mapPos.X < TerrainHelper.maxMapPixelX ||
                                mapPos.Y >= TerrainHelper.minMapPixelY || mapPos.Y < TerrainHelper.maxMapPixelY) {
                                    streamingWorldOwner.TeleportToCoordinates(mapPos.X, mapPos.Y);
                            } else {
                                l.log("Requested location is out of bounds!\n");
                            }
                        }
                    }
                    break;
                case (SNOW_COMMAND): 
                    if (weatherManager.IsSnowing) { 
                        weatherManager.StopSnowing();
                    } else {
                        weatherManager.StartSnowing();
                    }
                    break;
                case (XML_DEBUG):
                    QuestManager.GetInstance().doDebugQuest();
                    break;
                case (QUEST_DEBUG):
                    l.log("Dumping all quests:");
                    QuestManager.GetInstance().dumpAllQuests();
                    break;
                case (TIME_DEBUG):
                    l.log("The time is: " + dfUnity.WorldTime.Now.LongDateTimeString());
                    ulong timeInSeconds = dfUnity.WorldTime.Now.ToSeconds();
                    l.log("The time in seconds is: " + timeInSeconds.ToString());
                    timeInSeconds -= 60 * 60 * 24;
                    l.log("Setting the time 1 day in the past: " + timeInSeconds.ToString());
                    dfUnity.WorldTime.Now.FromSeconds(timeInSeconds);
                    l.log("The time is now: " + dfUnity.WorldTime.Now.LongDateTimeString());
                    l.log("The time in seconds is now: " + dfUnity.WorldTime.Now.ToSeconds().ToString());

                    break;
                default:
                    break;
            }


            return handled;
        }


        public void dispatchCommand(string command) {
            string[] args = command.Split(' ');
            displayText("> " + command);
            if (args.Length > 0) {
                parseCommand(args);
            }
            inputField.Select();
        }

        void drawFPS() {
            int w = Screen.width, h = Screen.height;
            Vector2 scrollPosition = Vector2.zero;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Dev Console");

            // FPS
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);

            /*GUILayout.BeginArea(new Rect(0, 50, w, h - 100));
            scrollPosition.y = Mathf.Infinity; // this seems silly (and will go away when input is implemented)
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(w), GUILayout.Height(h - 100));
            GUILayout.TextField(_outputText, "Label");
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            
            // TODO: SECURITY: userCommand should be validated 
            GUI.SetNextControlName("InputField");
            _userCommand = GUI.TextField(new Rect(0, h - 50, w, 50), _userCommand, 2048);*/
        }
    }
}