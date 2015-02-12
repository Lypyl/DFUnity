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
        string _userCommand = "";

        private const string TRAVEL_CMD = "travel";
        private const string SPAWN_ENEMY_CMD = "spawn_enemy";
        private const string SNOW_COMMAND = "snow";
        private const string XML_DEBUG = "xml";

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

            // TODO: DEBUG: REMOVE ME
            xml();
        }

        // Sanity check
        void Check() {
            if (!dfUnity) { 
                if(!DaggerfallUnity.FindDaggerfallUnity(out dfUnity)) { 
                    Logger.GetInstance().log(Error.ERROR_GAME_DAG_UNITY_NOT_FOUND + "\n");
                 
                }
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
            
            switch(args[0]) { 
                case (SPAWN_ENEMY_CMD):
                    if (args.Length == 2) {
                        int mobileType = System.Convert.ToInt32(args[1]);
                        if (mobileType > -1 && mobileType < 147) {
                            Logger.GetInstance().log("Spawning enemy of type " + (MobileTypes)mobileType + ".\n");
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
                            Logger.GetInstance().log("Unable to find location " + nameWithPossibleSpaces + ".\n");
                        } else {
                            Logger.GetInstance().log("Found location in " + location.RegionName + "!\n");
                            DFPosition mapPos = MapsFile.LongitudeLatitudeToMapPixel((int)location.MapTableData.Longitude, (int)location.MapTableData.Latitude);
                            if (mapPos.X >= TerrainHelper.minMapPixelX || mapPos.X < TerrainHelper.maxMapPixelX ||
                                mapPos.Y >= TerrainHelper.minMapPixelY || mapPos.Y < TerrainHelper.maxMapPixelY) {
                                    streamingWorldOwner.TeleportToCoordinates(mapPos.X, mapPos.Y);
                            } else {
                                Logger.GetInstance().log("Requested location is out of bounds!\n");
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
                    xml();
                    break;
                default:
                    break;
            }


            return handled;
        }

        // TODO: DEBUG: temporary. Move to an XML quest parser class
        private void xml() { 
            FileProxy managedFile = new FileProxy();
            managedFile.Load("Assets/Files/testquest.xml", FileUsage.UseMemory, true);
            XmlTextReader reader = new XmlTextReader(managedFile.GetStreamReader());
            Logger l = Logger.GetInstance();
            Dictionary<string, string> QRC = new Dictionary<string, string>();
            List<KeyValuePair<string, string>> QBN = new List<KeyValuePair<string, string>>();
            List<Task> tasks = new List<Task>();
            while (reader.Read()) {
                switch (reader.NodeType) { 
                    case XmlNodeType.Element:
                        if (reader.Name == "QRC") {
                            parseQRC(reader, out QRC);
                        } else if (reader.Name == "QBN") {
                            parseQBN(reader, out QBN, out tasks);
                        } else { 
                            l.log("<" + reader.Name + ">");
                        }
                        while (reader.MoveToNextAttribute()) {
                            l.log(" " + reader.Name + "='" + reader.Value + "'");
                        }
                        break;
                    case XmlNodeType.Text:
                        l.log(reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        l.log("</" + reader.Name + ">");
                        break;
                    case XmlNodeType.CDATA:
                        l.log("<![CDATA[" + reader.Value + "]]>");
                        break;
                    default:
                        l.log("unknown element");
                        break;
                }
            }

            foreach (KeyValuePair<string, string> kvp in QRC) { 
                l.log("sid: " + kvp.Key + ". Value: " + kvp.Value);
            }

            foreach (KeyValuePair<string, string> kvp in QBN) { 
                l.log("tag: " + kvp.Key + ". Value: " + kvp.Value);
            }

            foreach (Task task in tasks) {
                l.log("On task.");
                foreach (KeyValuePair<string, string> attribute in task.attributes) {
                    l.log("Attribute: " + attribute.Key + ", value: " + attribute.Value);
                }
                foreach (string command in task.commands) {
                    l.log("Command: " + command);
                }

            }
        }

        /** 
         * Takes an XmlTextReader and a dictionary to store the data and goes through it, stripping out all QRC data
         * The reader is advanced until </QRC> is found
         * Data is of the form <text sid="someid"> value </text>
         * <text> without sid is invalid data
         * The function will dutifully insert invalid data
         * @returns a bool indicating whether invalid data was found
         **/
        private bool parseQRC(XmlTextReader reader, out Dictionary<string, string> QRC) {
            bool succ = true;
            bool hitEndingQRC = false;

            QRC = new Dictionary<string, string>();
            string sid = "";
            string text = "";

            while (reader.Read()) {
                switch (reader.NodeType) { 
                    case XmlNodeType.Element:
                        if (reader.Name == "text" && reader.HasAttributes) {
                            sid = reader.GetAttribute("sid");
                        } 
                        break;
                    case XmlNodeType.Text:
                        text = reader.Value;
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "QRC") {
                            hitEndingQRC = true;
                            break;
                        } else if (reader.Name == "text") { 
                            if (sid == "" || text == "") { 
                                // got text without a sid! malformed quest file
                                succ = false;
                            }
                            QRC.Add(sid, text);
                            sid = "";
                            text = "";
                        }
                        break;
                    default:
                        break;
                }
                if (hitEndingQRC) break;
            }
            return succ;
        }

        class Task {
            public Dictionary<string, string> attributes = new Dictionary<string,string>();
            public List<string> commands = new List<string>();
        }
        
        /** 
         **/
        private bool parseQBN(XmlTextReader reader, out List<KeyValuePair<string, string>> QBN, out List<Task> tasks) {
            bool hitEndingQBN = false;
            bool succ = true;
            QBN = new List<KeyValuePair<string, string>>();
            tasks = new List<Task>();

            string key = "";
            string value = "";
            while (reader.Read()) {
                switch (reader.NodeType) { 
                    case XmlNodeType.Element:
                        if (reader.Name == "task") { 
                            tasks.Add(parseTask(reader));
                        } else { 
                            key = reader.Name;
                        }
                        break;
                    case XmlNodeType.Text:
                        value = reader.Value;
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "QBN") {
                            hitEndingQBN = true;
                        } else {
                            if (reader.Name != key) succ = false; // unmatching tag! super malformed XML file
                            QBN.Add(new KeyValuePair<string, string>(key, value));
                            key = "";
                            value = "";
                        }
                        break;
                    default:
                        break;
                }
                if (hitEndingQBN) break;
            }
            return succ; 
        }

        private Task parseTask(XmlTextReader reader) {
            Task task = new Task();
            if (reader.HasAttributes) { 
                while (reader.MoveToNextAttribute()) { 
                    task.attributes.Add(reader.Name, reader.Value);
                }
            }

            bool hitEndingTask = false;
            while (reader.Read()) { 
                switch (reader.NodeType) { 
                    case XmlNodeType.Element:
                        break;
                    case XmlNodeType.Text:
                        // Assumes all entries in a task are __c and thus commands
                        task.commands.Add(reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "task") {
                            hitEndingTask = true;
                        }
                        break;
                    default:
                        break;
                }
                if (hitEndingTask) break;
            }

            return task;
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