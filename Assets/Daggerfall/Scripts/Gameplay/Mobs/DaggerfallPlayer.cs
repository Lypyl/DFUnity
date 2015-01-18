using UnityEngine;
using System.Collections;

namespace Daggerfall.Gameplay.Mobs { 
    public class DaggerfallPlayer : Creature {
        public DaggerfallPlayer() : base(Creature.CREATURE_ID_DAGGERFALL_PLAYER) {
            attributes.setAttributes(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, false, false);
        }

        public string printDaggerfallPlayer() {
            return printCreature();
        }
    }

}
