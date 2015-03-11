using UnityEngine;
using System.Collections;
using Daggerfall.Game;

namespace Daggerfall.Game.UI { 
    public class ClickToDismiss : MonoBehaviour {
        ScrollManager scrollManager;

        // Use this for initialization
        void Start () {
            if (!scrollManager) { 
                scrollManager = GameObject.FindObjectOfType<ScrollManager>();
            }
        }
        
        // Update is called once per frame
        void Update () {
            if (Input.GetMouseButtonDown(0)) {
                scrollManager.closeScroll();
            }
        }
    }
}
