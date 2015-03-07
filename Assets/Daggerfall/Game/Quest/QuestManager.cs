using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using System.Collections.Generic;
using Daggerfall.Game.Events;

namespace Daggerfall.Game.Quest { 
    public class QuestManager : MonoBehaviour {
        private static QuestManager instance = null;
        System.Guid _UUID;
        private List<Quest> quests;
        private int _count = 0;

        private QuestManager() { 
            quests = new List<Quest>();
            _UUID = System.Guid.NewGuid();
        }

        public static QuestManager Instance {
            get {
                if(instance == null) {
                    GameObject go = new GameObject();
                    go.name = "QuestManager";
                    instance = go.AddComponent<QuestManager>();
                }
                return instance;
            }
        }

        public void timerComplete(GameTimerEvent gte) {

            if (gte.eventType == GameTimerEventType.GAME_TIMER_COMPLETE) {
                Logger.GetInstance().log("Timer just completed!");
                Logger.GetInstance().log(gte.gameTimer.dumpTimer());
            }
        }

        // Use this for initialization
        void Start () {
        }
        
        // Update is called once per frame
        void Update () {
            if(_count % 30 == 0) {
                foreach(Quest q in quests) {
                    q.updateGameTimers();
                }
            } else if (_count % 400 == 0) {
                foreach(Quest q in quests) { 
                    Logger.GetInstance().log(q.dumpAllTimers());
                }
                _count = 0;
            }
        
            _count += 1;
        }

        public void doDebugQuest() {
            Logger.GetInstance().log(_UUID.ToString());
            FileProxy file = new FileProxy();
            file.Load("Assets/Files/K0C00Y08.xml", FileUsage.UseMemory, true);
            Quest q; 
            XMLQuestParser.createQuestFromXMLFile(file, out q);
            quests.Add(q);
            Logger.GetInstance().log(q.dumpQuest());

            q.startAllTimers();
            Logger.GetInstance().log("Started all timers\n");
             Logger.GetInstance().log(q.dumpAllTimers());
        }

        public void dumpAllQuests() { 
            foreach (Quest q in quests) {
                q.dumpQuest();
                Logger.GetInstance().log(q.dumpAllTimers());
            }
        }
    }
}
