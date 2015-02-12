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
                if (!tmpEnemy) return; 

                GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                //Ray ray = new Ray(mainCamera.transform.position, Quaternion.Euler(45, 0, 0) * mainCamera.transform.forward);
                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                RaycastHit[] hits;
                hits = Physics.RaycastAll(ray, 100.0f);
                if (hits != null && hits.Length > 0) {
                    tmpEnemy.transform.position = hits[0].point;
                    tmpEnemy.transform.Translate(new Vector3(0, tmpEnemy.GetComponent<CharacterController>().height/2 + 1f));
                    //tmpEnemy.transform.Translate(0, 10f, 0);
                } else {
                    tmpEnemy.transform.position = player.transform.position;
                }

                Logger.GetInstance().log("Created a Daggerfall enemy GameObject.\n", this);
            }
        }
        
        // Update is called once per frame
        void Update () {
        
        }
    }
}
