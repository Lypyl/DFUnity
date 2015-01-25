using UnityEngine;
using System.Collections;
using Daggerfall.Gameplay.Mobs;

namespace Daggerfall.Gameplay { 
    public class DaggerfallPlayerController : MonoBehaviour {
        DaggerfallPlayer player;

        // Use this for initialization
        void Start () {
            player = new DaggerfallPlayer();
            string output = player.printDaggerfallPlayer();
            Logger.GetInstance().log("Created a new Daggerfall Player.");
        }
        
        // Update is called once per frame
        void Update () {
        
        }
    }
}