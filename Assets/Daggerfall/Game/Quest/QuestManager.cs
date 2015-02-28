using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using System.Collections.Generic;

namespace Daggerfall.Game.Quest { 
    public class QuestManager {
        private static QuestManager instance = null;
        System.Guid _UUID;
        private List<Quest> quests;

        private QuestManager() { 
            quests = new List<Quest>();
            _UUID = System.Guid.NewGuid();
        }

        public static QuestManager Instance {
            get {
                if(instance == null) {
                    instance = new QuestManager();
                }
                return instance;
            }
        }

        // Use this for initialization
        void Start () {
        }
        
        // Update is called once per frame
        void Update () {
        
        }

        public void doDebugQuest() {
            Logger.GetInstance().log(_UUID.ToString());
            FileProxy file = new FileProxy();
            file.Load("Assets/Files/testquest.xml", FileUsage.UseMemory, true);
            Quest q; 
            XMLQuestParser.createQuestFromXMLFile(file, out q);
            quests.Add(q);
            Logger.GetInstance().log(q.dumpQuest());
        }

        public void dumpAllQuests() { 
            Logger.GetInstance().log(_UUID.ToString());
            foreach (Quest q in quests) {
                q.dumpQuest();
                q.dumpAllTimers();
            }
        }
    }
}
