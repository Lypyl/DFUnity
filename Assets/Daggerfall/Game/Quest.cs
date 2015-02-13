using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Daggerfall.Game { 
    public class Quest : MonoBehaviour {
        public Dictionary<string, string> resources = new Dictionary<string, string>();
        public List<KeyValuePair<string, string>> QBN = new List<KeyValuePair<string, string>>();
        public List<Task> tasks = new List<Task>();
        public List<string> cdata = new List<string>();

        // Use this for initialization
        void Start () {
        
        }
        
        // Update is called once per frame
        void Update () {
        
        }

        public string dumpQuest() {
            string output = "";
            foreach (string datum in cdata) { 
                output += "CDATA: " + datum;
             }

            foreach (KeyValuePair<string, string> kvp in resources) { 
                output += "sid: " + kvp.Key + ". Value: " + kvp.Value;
            }

            foreach (KeyValuePair<string, string> kvp in QBN) { 
                output += "tag: " + kvp.Key + ". Value: " + kvp.Value;
            }

            foreach (Task task in tasks) {
                output += "On task.";
                foreach (KeyValuePair<string, string> attribute in task.attributes) {
                    output += "Attribute: " + attribute.Key + ", value: " + attribute.Value;
                }
                foreach (string command in task.commands) {
                    output += "Command: " + command;
                }
            }
            return output;
        }
    }
}
