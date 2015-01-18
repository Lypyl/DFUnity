using Daggerfall.Gameplay;

namespace Daggerfall.Gameplay.Mobs { 
    public class Creature {
        public const int CREATURE_ID_DAGGERFALL_PLAYER = 1;

        public const int MAX_LEVEL = 100;
        protected Attributes attributes;
        public bool enabled = false; // a disabled creture shouldn't be considered for some game logic and UI updates

        System.Guid _UUID;
        int creatureID;

        /**
         * Creates a new creature. Creature.enabled will be false until you set it
         *
         * @param _creatureID This creature's ID (not UUID)
         **/
        public Creature(int _creatureID = -1) {
            attributes = new Attributes();
            _UUID = System.Guid.NewGuid();
            creatureID = _creatureID;
        }

        /** 
         * Levels the creature up
         * 
         * @return Returns false is the creature is already at Creature.MAX_LEVEL. Performs the level up and returns true otherwise 
         **/
        public bool levelUp() {
            if (attributes.level < MAX_LEVEL) { 
                ++attributes.level;
                return true;
            } else { 
                return false;
            }
        }

        /** 
         * Levels the creature down
         * 
         * @return Returns false is the creature is already at 1. Performs the level down and returns true otherwise
         **/
        public bool levelDown() { 
            if (attributes.level > 0) { 
                --attributes.level;
                return true;
            } else { 
                return false;
            }
        }

        /**
         * Kills the creature
         * 
         * @todo Dispatch event? 
         **/
        public void kill() {
            attributes.health = 0;

            // TODO: ramifications of a creature dying
            // e.g., the game may say "Giant Rat just died."

        }

        /**
         * Heals the creature to attributes.maxHealth
         **/
        public void healFully() {
            attributes.health = attributes.maxHealth;
        }

        /**
         * Heals or injures the creature, killing it if appropriate
         * 
         * @param deltaHealth The positive or negative value to add to attributes.health
         **/
        public void healOrInjure(float deltaHealth) {
            attributes.health += deltaHealth;
            if (attributes.health < 0) { 
                attributes.health = 0;
                kill();
            }
            else if (attributes.health > attributes.maxHealth) { 
                attributes.health = attributes.maxHealth;
            }
        }

        public int getCreatureID() {
            return creatureID;
        }

        /**
         * Sets the creatureID for this creature. You should set this at creation time and might not want to do this.
         *
         *  @param _creatureID The ID of this creature (different from the UUID, which you cannot set)
         **/
        public void setCreatureID(int _creatureID) {
            creatureID = _creatureID;
        }

        /**
         * @returns Returns a string representation of the UUID (System.GUID) of this creature
         **/
        public string getUUID() {
            return _UUID.ToString();
        }

        public bool isSleeping() {
            return attributes.sleeping;
        }

        public bool isSneaking() {
            return attributes.sneaking;
        }

        public void sleep() {
            attributes.sleeping = true;
        }

        public void awaken() {
            attributes.sleeping = false;
        }

        public void startSneaking() {
            attributes.sneaking = true;
        }

        public void stopSneaking() {
            attributes.sneaking = false;
        }

        /**
         * @returns a comprehensive string of information about this creature
         */
        protected string printCreature() {
            string s = "\n*** Creature ***\n UUID: " + _UUID.ToString() + "\n CreatureID: " + creatureID + "\n Enabled: " + enabled + "\n";
            s += attributes.printAttributes();
            return s;
        }

    }
}