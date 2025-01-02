using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PowerfulUI
{
    [CustomEditor(typeof(TabGroup))]
    [CanEditMultipleObjects]
    public class TabGroupEditor : Editor
    {
        SerializedProperty m_IndexProperty;
        
        private void OnEnable()
        {
            m_IndexProperty = serializedObject.FindProperty("m_Index");
        }
        

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Index"));
            if (EditorGUI.EndChangeCheck())
            {
                for (var i = 0; i < targets.Length; i ++)
                {
                    ((TabGroup)targets[i]).index = m_IndexProperty.intValue;
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ValueChangeOnClick"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnClick"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnValueChanged"));
            serializedObject.ApplyModifiedProperties();
        }
    }

}
