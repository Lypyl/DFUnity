using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using System.Collections.Generic;

namespace Daggerfall.Game.Quest { 
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
            Quest q; 
            XMLQuestParser.createQuestFromXMLFile(file, out q);
            Logger.GetInstance().log(q.dumpQuest());
        }


        
    }
}
