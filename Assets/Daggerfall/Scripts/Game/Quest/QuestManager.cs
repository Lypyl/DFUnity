using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using System.Collections.Generic;
using Daggerfall.Game.Events;

namespace Daggerfall.Game.Quest { 
    public class QuestManager : MonoBehaviour {
        private static QuestManager instance = null;
        public UIManager uiManager;
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
                    //instance.Start();
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
        void Awake() {
            if (!uiManager) {
                uiManager = GameObject.FindGameObjectWithTag("UIOwner").GetComponentInChildren<UIManager>();
            }
        }
        
        // Update is called once per frame
        /**
         * Every few ticks this function will update any running quests
         **/
        void Update () {
            // TODO: DEBUG: DEMO: Remove this 
            if (Input.GetButtonDown("TempDebugKey")) {  
                doDebugQuest();
                return;
            }


            if (uiManager.isUIOpen) return;
            if(_count % 32 == 0) {
                foreach(Quest q in quests) {
                    q.updateGameTimers();
                    uiManager.SendMessage("debugHUD_displayText", q.dumpAllRunningTimers());
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
            uiManager.SendMessage("uiScroll_displayText", q.resources["QuestorOffer"]);
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
