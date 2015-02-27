using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using DaggerfallConnect;
using DaggerfallConnect.Utility;

namespace Daggerfall.Game.Quest { 
    public static class XMLQuestParser {

        public static void createQuestFromXMLFile(FileProxy xmlFile, out Quest q) { 
            XmlTextReader reader = new XmlTextReader(xmlFile.GetStreamReader());
            q = new Quest();
            Logger l = Logger.GetInstance();

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
                        break;
                    case XmlNodeType.Text:
                        l.log(reader.Value);
                        break;
                    case XmlNodeType.CDATA:
                        q.cdata.Add(reader.Value);
                        break;
                    case XmlNodeType.Whitespace:
                        break;
                    default:
                        l.log("unknown element: " + reader.Name + ", " + reader.Value);
                        break;
                }
            }

            q.createTimers();
            l.log("Created all timers\n");
            q.startAllTimers();
            l.log("Started all timers\n");
            q.dumpAllTimers();
        }

        /** 
         * Takes an XmlTextReader and a dictionary to store the data and goes through it, stripping out all QRC data
         * The reader is advanced until </QRC> is found
         * Data is of the form <text sid="someid"> value </text>
         * <text> without sid is invalid data
         * The function will dutifully insert invalid data
         * @returns a bool indicating whether invalid data was found
         **/
        private static bool parseQRC(XmlTextReader reader, out Dictionary<string, string> QRC) {
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
        private static bool parseQBN(XmlTextReader reader, out List<KeyValuePair<string, string>> QBN, out List<Task> tasks) {
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

        private static Task parseTask(XmlTextReader reader) {
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
