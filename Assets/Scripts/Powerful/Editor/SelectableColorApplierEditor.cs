using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace Powerful
{
    [CustomEditor(typeof(SelectableColorApplier), true)]
    [CanEditMultipleObjects]
    public class SelectableColorApplierEditor : Editor
    {
        SerializedProperty m_Script;
        SerializedProperty m_TargetGraphicProperty;
        SerializedProperty m_ColorBlockProperty;
        
        // Whenever adding new SerializedProperties to the Selectable and SelectableEditor
        // Also update this guy in OnEnable. This makes the inherited classes from Selectable not require a CustomEditor.
        private string[] m_PropertyPathToExcludeForChildClasses;

        protected virtual void OnEnable()
        {
            m_Script                = serializedObject.FindProperty("m_Script");
            m_TargetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
            m_ColorBlockProperty    = serializedObject.FindProperty("m_Colors");

            m_PropertyPathToExcludeForChildClasses = new[]
            {
                m_Script.propertyPath,
                m_ColorBlockProperty.propertyPath,
                m_TargetGraphicProperty.propertyPath,
            };
        }

        protected virtual void OnDisable()
        {
        }

        static UnityEngine.UI.Selectable.Transition GetTransition(SerializedProperty transition)
        {
            return (UnityEngine.UI.Selectable.Transition)transition.enumValueIndex;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var graphic = m_TargetGraphicProperty.objectReferenceValue as Graphic;
            if (graphic == null)
                graphic = (target as SelectableColorApplier).GetComponent<Graphic>();

            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.PropertyField(m_TargetGraphicProperty);

                if (graphic == null)
                    EditorGUILayout.HelpBox("You must have a Graphic target in order to use a color transition.", MessageType.Warning);

                EditorGUILayout.PropertyField(m_ColorBlockProperty);
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.Space();

            // We do this here to avoid requiring the user to also write a Editor for their Selectable-derived classes.
            // This way if we are on a derived class we dont draw anything else, otherwise draw the remaining properties.
            ChildClassPropertiesGUI();

            serializedObject.ApplyModifiedProperties();
        }

        // Draw the extra SerializedProperties of the child class.
        // We need to make sure that m_PropertyPathToExcludeForChildClasses has all the Selectable properties and in the correct order.
        // TODO: find a nicer way of doing this. (creating a InheritedEditor class that automagically does this)
        private void ChildClassPropertiesGUI()
        {
            if (IsDerivedSelectableEditor())
                return;

            DrawPropertiesExcluding(serializedObject, m_PropertyPathToExcludeForChildClasses);
        }

        private bool IsDerivedSelectableEditor()
        {
            return GetType() != typeof(SelectableEditor);
        }
    }

}