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
    [CustomEditor(typeof(DaggerfallBillboardBatch))]
    public class DaggerfallBillboardBatchEditor : Editor
    {
        private DaggerfallBillboardBatch dfBillboardBatch { get { return target as DaggerfallBillboardBatch; } }

        SerializedProperty Prop(string name)
        {
            return serializedObject.FindProperty(name);
        }

        public override void OnInspectorGUI()
        {
            // Update
            serializedObject.Update();

            DisplayGUI();

            // Save modified properties
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        public void DisplayGUI()
        {
            DrawDefaultInspector();

            GUILayoutHelper.Horizontal(() =>
            {
                if (GUILayout.Button("Clear"))
                {
                    dfBillboardBatch.__EditorClearBillboards();
                }
                if (GUILayout.Button("Random"))
                {
                    dfBillboardBatch.__EditorRandomLayout();
                }
            });
        }
    }
}