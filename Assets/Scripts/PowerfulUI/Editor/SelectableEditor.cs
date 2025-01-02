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
        private SerializedProperty m_OnClickProperty;
        private SerializedProperty m_EnableLongPressProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_EnableLongPressProperty = serializedObject.FindProperty("m_EnableLongPress");
        }

        public override void OnInspectorGUI()
        {
            selectableFoldout = EditorGUILayout.Foldout(selectableFoldout, "Selectable", EditorStyles.foldoutHeader);
            if (selectableFoldout == true)
            {
                EditorGUI.indentLevel ++;
                base.OnInspectorGUI();
                EditorGUILayout.Space();

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
            }
        }
    }
}
