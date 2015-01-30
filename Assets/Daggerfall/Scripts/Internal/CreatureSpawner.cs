using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using Daggerfall.Gameplay.Mobs;

namespace Daggerfall.Internal { 
    public class CreatureSpawner : MonoBehaviour {
        DaggerfallUnity dfUnity;
        GameObject tmpEnemy;

        // Use this for initialization
        void Start () {
        }

        public void SpawnEnemy(MobileTypes mobileType) { 
            if (DaggerfallUnity.FindDaggerfallUnity(out dfUnity)) { 
                Logger.GetInstance().log("Creating a Daggerfall enemy GameObject.\n", this);
                tmpEnemy = GameObjectHelper.CreateDaggerfallEnemyGameObject(mobileType, this.transform, MobileReactions.Hostile);
                Logger.GetInstance().log("Created a Daggerfall enemy GameObject.\n", this);
            }
        }
        
        // Update is called once per frame
        void Update () {
        
        }
    }
}
