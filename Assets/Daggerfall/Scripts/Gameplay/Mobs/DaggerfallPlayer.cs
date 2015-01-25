using UnityEngine;
using System.Collections;

namespace Daggerfall.Gameplay.Mobs { 
    public class DaggerfallPlayer : Creature {
        public DaggerfallPlayer() : base() { 
            attributes.setAttributes(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, false, false);
        }

        public string printDaggerfallPlayer() {
            return printCreature();
        }
    }

}
