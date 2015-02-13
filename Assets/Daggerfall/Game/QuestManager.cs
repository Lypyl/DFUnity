using UnityEngine;
using System.Collections;
using System.Xml;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using System.Collections;
using System.Collections.Generic;

namespace Daggerfall.Game { 
    public class QuestManager : MonoBehaviour {

        private static QuestManager instance;
        public static QuestManager GetInstance() { 
            if (instance == null) {
                instance = new QuestManager();
            }
            return instance;
        }

        // Use this for initialization
        void Start () {
        
        }
        
        // Update is called once per frame
        void Update () {
        
        }

        public void doDebugQuest() { 
            FileProxy file = new FileProxy();
            file.Load("Assets/Files/testquest.xml", FileUsage.UseMemory, true);
            createQuestFromXMLFile(file);
        }

        private void createQuestFromXMLFile(FileProxy xmlFile) { 
            XmlTextReader reader = new XmlTextReader(xmlFile.GetStreamReader());

            Logger l = Logger.GetInstance();
            Quest q = new Quest();

            while (reader.Read()) {
                switch (reader.NodeType) { 
                    case XmlNodeType.Element:
                        if (reader.Name == "QRC") {
                            parseQRC(reader, out q.resources);
                        } else if (reader.Name == "QBN") {
                            parseQBN(reader, out q.QBN, out q.tasks);
                        } else { 
                            //l.log("<" + reader.Name + ">");
                        }
                        while (reader.MoveToNextAttribute()) {
                            //l.log(" " + reader.Name + "='" + reader.Value + "'");
                        }
                        break;
                    case XmlNodeType.Text:
                        l.log(reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        //l.log("</" + reader.Name + ">");
                        break;
                    case XmlNodeType.CDATA:
                        //l.log("<![CDATA[" + reader.Value + "]]>");
                        q.cdata.Add(reader.Value);
                        break;
                    case XmlNodeType.Whitespace:
                        break;
                    default:
                        l.log("unknown element: " + reader.Name + ", " + reader.Value);
                        break;
                }
            }

            l.log(q.dumpQuest());

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
    }
}
