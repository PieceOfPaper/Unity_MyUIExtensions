using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PowerfulUI
{
    [CustomEditor(typeof(Selectable), true)]
    [CanEditMultipleObjects]
    public class SelectableEditor : UnityEditor.UI.SelectableEditor
    {
        public bool selectableFoldout = false;
        public bool eventsFoldout = false;
        
        protected SerializedProperty m_ScriptProperty;
        protected SerializedProperty m_OnClickProperty;
        protected SerializedProperty m_EnableLongPressProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ScriptProperty = serializedObject.FindProperty("m_Script");
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_EnableLongPressProperty = serializedObject.FindProperty("m_EnableLongPress");
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(m_ScriptProperty);
            
            selectableFoldout = EditorGUILayout.Foldout(selectableFoldout, "Selectable", EditorStyles.foldoutHeader);
            if (selectableFoldout == true)
            {
                EditorGUI.indentLevel ++;
                base.OnInspectorGUI();
                EditorGUI.indentLevel --;
            }
            
            eventsFoldout = EditorGUILayout.Foldout(eventsFoldout, "Events", EditorStyles.foldoutHeader);
            if (eventsFoldout == true)
            {
                EditorGUI.indentLevel ++;
                serializedObject.Update();
                EditorGUILayout.PropertyField(m_OnClickProperty);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSubmitToClick"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EnableEventOnDisabled"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_EnableLongPressProperty);
                if (m_EnableLongPressProperty.hasMultipleDifferentValues || m_EnableLongPressProperty.boolValue == true)
                {
                    EditorGUI.indentLevel ++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnBeginLongPress"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEndLongPress"));
                    EditorGUI.indentLevel --;
                }
                serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel --;
                EditorGUILayout.Space();
            }
        }
    }
}
