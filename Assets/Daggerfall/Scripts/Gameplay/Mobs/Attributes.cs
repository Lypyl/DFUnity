using Daggerfall.Gameplay;

namespace Daggerfall.Gameplay.Mobs { 
    public class Attributes {

        public int strength, intelligence, will, agility, endurance, personality, speed, luck;
        public float health, maxHealth, mana, maxMana, stamina, maxStamina; 
        public int level;
        public Skills skills;
        public bool sleeping, sneaking;

        public Attributes() {
            skills = new Skills();
        }

        public void setAttributes(int _strength, int _intelligence, int _will, int _agility, int _endurance, int _personality, int _speed, int _luck, float _health, 
                                 float _maxHealth, float _mana, float _maxMana, float _stamina, float _maxStamina, int _level, bool _sleeping, bool _sneaking) {
            strength = _strength;
            intelligence = _intelligence;
            will = _will; 
            agility = _agility;
            endurance = _endurance;
            personality = _personality;
            speed = _speed;
            luck = _luck;
            health = _health;
            maxHealth = _maxHealth;
            mana = _mana;
            maxMana = _maxMana;
            stamina = _stamina;
            maxStamina = _maxStamina;
            level = _level;
            sleeping = _sleeping;
            sneaking = _sneaking;
        }

        ~Attributes() {
        }

        /**
         * @returns A comprehensive string of the attributes and Skills
        *
         */
        public string printAttributes() {
            string s = "";

            s += "  Health " + health + "/" + maxHealth + "\n";
            s += "  Mana " + mana + "/" + maxMana + " \n";
            s += "  Stamina " + stamina + "/" + maxStamina + "\n";

            s += skills.printSkills();

            s += "Strength " + strength + " -- ";
            s += "Intelligence " + intelligence + " -- ";
            s += "Willpower " + will + " -- ";
            s += "Agility " + agility + " -- ";
            s += "Endurance " + endurance + " -- ";
            s += "Personality " + personality + " -- ";
            s += "Speed " + speed + " -- ";
            s += "Luck " + luck + " -- ";
            s += "Level " + level + "\n";

            s += "  Sleeping: " + sleeping + " \n";
            s += "  Sneaking: " + sneaking + " \n";

            return s;
        }
    }
}