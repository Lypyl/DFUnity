using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.InternalTypes;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop
{
    [CustomEditor(typeof(DaggerfallBlock))]
    public class DaggerfallBlockEditor : Editor
    {
        private DaggerfallBlock dfBlock { get { return target as DaggerfallBlock; } }

        SerializedProperty Prop(string name)
        {
            return serializedObject.FindProperty(name);
        }

        public override void OnInspectorGUI()
        {
            // Update
            serializedObject.Update();

            // Save modified properties
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}
