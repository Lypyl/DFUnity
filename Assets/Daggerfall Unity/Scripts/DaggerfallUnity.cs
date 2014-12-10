#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// DaggerfallUnity main class.
    /// </summary>
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(WorldTime))]
    [RequireComponent(typeof(PlayerGPS))]
    [RequireComponent(typeof(MaterialReader))]
    [RequireComponent(typeof(MeshReader))]
    [RequireComponent(typeof(SoundReader))]
    public class DaggerfallUnity : MonoBehaviour
    {
        [NonSerialized]
        public const string Version = "1.1.0";

        #region Fields

        bool isReady = false;
        DaggerfallUnity instance;
        Dictionary<int, MobileEnemy> enemyDict;
        ContentReader reader;

        WorldTime worldTime;
        PlayerGPS playerGPS;
        MaterialReader materialReader;
        MeshReader meshReader;
        SoundReader soundReader;
        
        #endregion

        #region Public Fields

        public string Arena2Path;
        public int ModelImporter_ModelID = 456;
        public string BlockImporter_BlockName = "MAGEAA01.RMB";
        public string CityImporter_CityName = "Daggerfall/Daggerfall";
        public string DungeonImporter_DungeonName = "Daggerfall/Privateer's Hold";

        // Mesh combining options
        public bool Option_CombineRMB = true;
        public bool Option_CombineRDB = true;

        // Import options
        public bool Option_SetStaticFlags = true;
        public bool Option_AddMeshColliders = true;
        public bool Option_AddNavmeshAgents = true;
        public bool Option_DefaultSounds = true;
        public bool Option_CloseCityGates = false;

        // Light options
        public bool Option_ImportPointLights = false;
        public bool Option_AnimatedPointLights = false;
        public string Option_PointLightTag = "Untagged";
#if UNITY_EDITOR
        public MonoScript Option_CustomPointLightScript = null;
#endif

        // Enemy options
        public bool Option_ImportEnemies = true;
        public bool Option_EnemyCharacterController = false;
        public bool Option_EnemyRigidbody = false;
        public bool Option_EnemyCapsuleCollider = false;
        public bool Option_EnemyNavMeshAgent = false;
        public bool Option_EnemyExampleAI = false;
        public string Option_EnemyTag = "Untagged";
        public float Option_EnemyRadius = 0.4f;
        public float Option_EnemySlopeLimit = 80f;
        public float Option_EnemyStepOffset = 0.4f;
        public bool Option_EnemyUseGravity = false;
        public bool Option_EnemyIsKinematic = true;
#if UNITY_EDITOR
        public MonoScript Option_CustomEnemyScript = null;
#endif

        // Time and space options
        public bool Option_AutomateTextureSwaps = true;
        public bool Option_AutomateSky = true;
        public bool Option_AutomateCityWindows = true;
        public bool Option_AutomateCityLights = true;
        public bool Option_AutomateCityGates = false;

        #endregion

        #region Class Properties

        public bool IsReady
        {
            get { return isReady; }
        }

        public MaterialReader MaterialReader
        {
            get { return (materialReader != null) ? materialReader : materialReader = GetComponent<MaterialReader>(); }
        }

        public MeshReader MeshReader
        {
            get { return (meshReader != null) ? meshReader : meshReader = GetComponent<MeshReader>(); }
        }

        public SoundReader SoundReader
        {
            get { return (soundReader != null) ? soundReader : soundReader = GetComponent<SoundReader>(); }
        }

        public WorldTime WorldTime
        {
            get { return (worldTime != null) ? worldTime : worldTime = GetComponent<WorldTime>(); }
        }

        public PlayerGPS PlayerGPS
        {
            get { return (playerGPS != null) ? playerGPS : playerGPS = GetComponent<PlayerGPS>(); }
        }

        public ContentReader ContentReader
        {
            get
            {
                if (reader == null)
                    SetupContentReaders();
                return reader;
            }
        }

        public Dictionary<int, MobileEnemy> EnemyDict
        {
            get { return enemyDict; }
        }

        #endregion

        #region Unity

        void Start()
        {
            Setup();
            SetupSingleton();
            SetupContentReaders();
        }

        void Update()
        {
            // Instance must be set up
            if (!Setup())
                return;

#if UNITY_EDITOR
            // Content readers must be ready
            // This is checked every update in editor as
            // code changes can reset singleton fields
            SetupContentReaders();
#endif
        }

        #endregion

        #region Startup and Shutdown

        public bool Setup()
        {
            // Full validation is only performed in editor mode
            // This is to allow standalone builds to start with
            // no Arena2 data, or partial Arena2 data in Resources
            if (!isReady)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    // Must have a path set
                    if (string.IsNullOrEmpty(Arena2Path))
                        return false;

                    // Validate current path
                    if (ValidateArena2Path(Arena2Path))
                    {
                        isReady = true;
                        LogMessage("Arena2 path validated.", true);
                        SetupSingleton();
                        SetupContentReaders();
                    }
                    else
                    {
                        isReady = false;
                        return false;
                    }
                }
                else
                {
                    SetupSingleton();
                    SetupContentReaders();
                }
#else
                SetupSingleton();
                SetupContentReaders();
#endif

                isReady = true;
            }

            return true;
        }

        public bool ValidateArena2Path(string path)
        {
            DFValidator.ValidationResults results;
            DFValidator.ValidateArena2Folder(path, out results);

            return results.AppearsValid;
        }

        private void SetupSingleton()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
            {
                if (Application.isPlaying)
                {
                    LogMessage("Multiple DaggerfallUnity instances detected!", true);
                    Destroy(gameObject);
                }
            }
        }

        private void SetupContentReaders()
        {
            if (isReady)
            {
                if (reader == null)
                    reader = new ContentReader(Arena2Path, this);
                if (enemyDict == null)
                    enemyDict = EnemyBasics.GetEnemyDict();
            }
        }

        #endregion

        #region Public Static Methods

        public static void LogMessage(string message, bool showInEditor = false)
        {
            if (showInEditor || Application.isPlaying) Debug.Log(string.Format("DaggerfallUnity {0}: {1}", Version, message));
        }

        public static bool FindDaggerfallUnity(out DaggerfallUnity dfUnityOut)
        {
            dfUnityOut = GameObject.FindObjectOfType(typeof(DaggerfallUnity)) as DaggerfallUnity;
            if (dfUnityOut == null)
            {
                LogMessage("Could not locate DaggerfallUnity GameObject instance in scene!", true);
                return false;
            }
            else
            {
                if (!dfUnityOut.IsReady)
                {
                    dfUnityOut = null;
                    LogMessage("Found DaggerfallUnity instance but it is not ready. Have you set your Arena2 path?", false);
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
