using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Powerful
{
    [CustomEditor(typeof(Tab))]
    [CanEditMultipleObjects]
    public class TabEditor : SelectableEditor
    {
        SerializedProperty m_GroupProperty;
        SerializedProperty m_IsOnProperty;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            m_GroupProperty = serializedObject.FindProperty("m_Group");
            m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
        }
        

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Index"));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Group"));
            if (EditorGUI.EndChangeCheck())
            {
                for (var i = 0; i < targets.Length; i ++)
                {
                    ((Tab)targets[i]).group = m_GroupProperty.objectReferenceValue == null ? null : (TabGroup)m_GroupProperty.objectReferenceValue;
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Ons"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Offs"));
            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(m_GroupProperty.objectReferenceValue !=  null))
                EditorGUILayout.PropertyField(m_IsOnProperty);
            if (EditorGUI.EndChangeCheck())
            {
                for (var i = 0; i < targets.Length; i ++)
                {
                    ((Tab)targets[i]).isOn = m_IsOnProperty.boolValue;
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnValueChanged"));
            serializedObject.ApplyModifiedProperties();
        }
    }

}
