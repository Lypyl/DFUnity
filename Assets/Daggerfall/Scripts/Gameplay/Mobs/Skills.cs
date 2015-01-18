using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daggerfall.Gameplay.Mobs {
    public class Skills {

        // Skills range from 0.0f to 100.0f
        public float Alteration, Archery, Axe, Backstabbing, BluntWeapon, Centaurian, Climbing, CriticalStrike, Daedric, Destruction, Disguise, Dodging, 
              Dragonish, Elvish, Etiquette, Faerie, Giantish, HandtoHand, Harpy, Illusion, Impish, Jumping, Lockpicking, LongBlade, Medical, 
              Mercantile, Mysticism, Nymph, Orcish, Pickpocket, Restoration, Running, ShortBlade, Spriggan, Stealth, Streetwise, Swimming, Thaumaturgy;

        public Skills() {
            setAllSkillsToValue(1.0f);
        }

        void setAllSkillsToValue(float value = 1.0f) {
            Alteration = value;
            Archery = value;
            Axe = value;
            Backstabbing = value;
            BluntWeapon = value;
            Centaurian = value;
            Climbing = value;
            CriticalStrike = value;
            Daedric = value;
            Destruction = value;
            Disguise = value;
            Dodging = value;
            Dragonish = value;
            Elvish = value;
            Etiquette = value;
            Faerie = value;
            Giantish = value;
            HandtoHand = value;
            Harpy = value;
            Illusion = value;
            Impish = value;
            Jumping = value;
            Lockpicking = value;
            LongBlade = value;
            Medical = value;
            Mercantile = value;
            Mysticism = value;
            Nymph = value;
            Orcish = value;
            Pickpocket = value;
            Restoration = value;
            Running = value;
            ShortBlade = value;
            Spriggan = value;
            Stealth = value;
            Streetwise = value;
            Swimming = value;
            Thaumaturgy = value;
        }

        /**
         * @returns A string representation of all the skills
         **/
        public string printSkills() {
            string s = "\n";
            s += "Alteration: " + Alteration + "% -- ";
            s += "Archery: " + Archery + "% -- ";
            s += "Axe: " + Axe + "% -- ";
            s += "Backstabbing: " + Backstabbing + "% -- ";
            s += "Blunt Weapon: " + BluntWeapon + "% -- ";
            s += "Centaurian: " + Centaurian + "% -- ";
            s += "Climbing: " + Climbing + "% -- ";
            s += "Critical Strike: " + CriticalStrike + "% -- ";
            s += "Daedric: " + Daedric + "% -- ";
            s += "Destruction: " + Destruction + "% -- ";
            s += "Disguise: " + Disguise + "% -- ";
            s += "Dodging: " + Dodging + "% -- ";
            s += "Dragonish: " + Dragonish + "% -- ";
            s += "Elvish: " + Elvish + "% -- ";
            s += "Etiquette: " + Etiquette + "% -- ";
            s += "Faerie: " + Faerie + "% -- ";
            s += "Giantish: " + Giantish + "% -- ";
            s += "Hand-to-Hand: " + HandtoHand + "% -- ";
            s += "Harpy: " + Harpy + "% -- ";
            s += "Illusion: " + Illusion + "% -- ";
            s += "Impish: " + Impish + "% -- ";
            s += "Jumping: " + Jumping + "% -- ";
            s += "Lockpicking: " + Lockpicking + "% -- ";
            s += "Long Blade: " + LongBlade + "% -- ";
            s += "Medical: " + Medical + "% -- ";
            s += "Mercantile: " + Mercantile + "% -- ";
            s += "Mysticism: " + Mysticism + "% -- ";
            s += "Nymph: " + Nymph + "% -- ";
            s += "Orcish: " + Orcish + "% -- ";
            s += "Pickpocket: " + Pickpocket + "% -- ";
            s += "Restoration: " + Restoration + "% -- ";
            s += "Running: " + Running + "% -- ";
            s += "Short Blade: " + ShortBlade + "% -- ";
            s += "Spriggan: " + Spriggan + "% -- ";
            s += "Stealth: " + Stealth + "% -- ";
            s += "Streetwise: " + Streetwise + "% -- ";
            s += "Swimming: " + Swimming + "% -- ";
            s += "Thaumaturgy: " + Thaumaturgy + "% -- \n";
            return s;
        } 
    }
}
