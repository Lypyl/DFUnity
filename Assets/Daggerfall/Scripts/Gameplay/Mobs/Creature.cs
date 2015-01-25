using UnityEngine;
using Daggerfall.Gameplay;
using DaggerfallWorkshop.Utility;
using System.Collections.Generic;
using DaggerfallWorkshop;

namespace Daggerfall.Gameplay.Mobs { 
    public class Creature : MonoBehaviour { 
        // TODO: Just use what's in the EnemyBasics class
        public const int CREATURE_ID_DAGGERFALL_PLAYER = 1;

        public const int MAX_LEVEL = 100;
        protected Attributes attributes;
        //public bool enabled = false; // a disabled creture shouldn't be considered for some game logic and UI updates

        System.Guid _UUID;
        MobileTypes creatureType;
        MobileBehaviour behaviour;
        DaggerfallUnity dfUnity;

        void Start() { 

        }

        void Awake() {
            attributes = new Attributes();
            _UUID = System.Guid.NewGuid();

            // TODO: Sane defaults?
            creatureType = MobileTypes.Acrobat;
            behaviour = MobileBehaviour.General;

            DaggerfallUnity.FindDaggerfallUnity(out dfUnity);
            //setupMobile();
        }


        public void setupMobile() { 
            name = string.Format("DaggerfallEnemy [{0}]", creatureType.ToString());

            // Add child object for enemy billboard
            GameObject mobileObject = new GameObject("DaggerfallMobileUnit");
            mobileObject.transform.parent = this.transform;

            // Add mobile enemy
            Vector2 size = Vector2.one;
            DaggerfallMobileUnit dfMobile = mobileObject.AddComponent<DaggerfallMobileUnit>();
            try {
                dfMobile.SetEnemy(dfUnity, dfUnity.EnemyDict[(int)creatureType]);
                size = dfMobile.Summary.RecordSizes[0];
            } catch(System.Exception e) {
                string message = string.Format("Failed to set enemy type (int)type={0}. '{1}'", (int)creatureType, e.Message);
                // TODO: Change logging
                DaggerfallUnity.LogMessage(message);
                GameObject.DestroyImmediate(dfMobile);
            }

            // Add character controller
            if (dfUnity.Option_EnemyCharacterController) {
                CharacterController controller = gameObject.AddComponent<CharacterController>();
                controller.radius = dfUnity.Option_EnemyRadius;
                controller.height = size.y;
                controller.slopeLimit = dfUnity.Option_EnemySlopeLimit;
                controller.stepOffset = dfUnity.Option_EnemyStepOffset;

                // Reduce height of flying creatures as their wing animation makes them taller than desired
                // This helps them get through doors while aiming for player eye height
                if (dfMobile.Summary.Enemy.Behaviour == MobileBehaviour.Flying)
                    controller.height /= 2f;

                // Limit maximum height to ensure controller can fit through doors
                // For some reason Unity 4.5 doesn't let you set SkinWidth from code >.<
                if (controller.height > 1.9f)
                    controller.height = 1.9f;
            }

            // Add rigidbody
            if (dfUnity.Option_EnemyRigidbody) {
                Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = dfUnity.Option_EnemyUseGravity;
                rigidbody.isKinematic = dfUnity.Option_EnemyIsKinematic;
            }

            // Add capsule collider
            if (dfUnity.Option_EnemyCapsuleCollider) {
                CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
                collider.radius = dfUnity.Option_EnemyRadius;
                collider.height = size.y;
            }

            // Add navmesh agent
            if (dfUnity.Option_EnemyNavMeshAgent) {
                NavMeshAgent agent = gameObject.AddComponent<NavMeshAgent>();
                agent.radius = dfUnity.Option_EnemyRadius;
                agent.height = size.y;
                agent.baseOffset = size.y * 0.5f;
            }

            // Add example AI
            if (dfUnity.Option_EnemyExampleAI)
            {
                // EnemyMotor will also add other required components
                gameObject.AddComponent<DaggerfallWorkshop.Demo.EnemyMotor>();

                // Set sounds
                DaggerfallWorkshop.Demo.EnemySounds enemySounds = gameObject.GetComponent<DaggerfallWorkshop.Demo.EnemySounds>();
                if (enemySounds)
                {
                    enemySounds.MoveSound = (SoundClips)dfMobile.Summary.Enemy.MoveSound;
                    enemySounds.BarkSound = (SoundClips)dfMobile.Summary.Enemy.BarkSound;
                    enemySounds.AttackSound = (SoundClips)dfMobile.Summary.Enemy.AttackSound;
                }
            }
        }

        void Update() { 

        }

        public Creature(MobileTypes _creatureType = MobileTypes.Acrobat) {
            //attributes = new Attributes();
            //creatureType = _creatureType;
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

        public MobileTypes getCreatureType() {
            return creatureType;
        }

        /**
         * Sets the creatureType for this creature. You should set this at creation time and might not want to do this.
         *
         *  @param _creatureType The ID of this creature (different from the UUID, which you cannot set)
         **/
        public void setCreatureType(MobileTypes _creatureType) {
            creatureType = _creatureType;
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
        public string printCreature() {
            string s = "\n*** Creature ***\n UUID: " + _UUID.ToString() + "\n CreatureID: " + creatureType.ToString() + "\n Enabled: " + enabled + "\n";
            s += attributes.printAttributes();
            return s;
        }

    }
}