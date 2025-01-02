using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;

namespace PowerfulUI
{
    [CustomEditor(typeof(Selectable), true)]
    [CanEditMultipleObjects]
    public class SelectableEditor : UnityEditor.UI.SelectableEditor
    {
        public bool transitionFoldout { get => EditorPrefs.GetBool("PowerfulUI.SelectableEditor.transitionFoldout", false); set => EditorPrefs.SetBool("PowerfulUI.SelectableEditor.transitionFoldout", value); }
        public bool navigationFoldout { get => EditorPrefs.GetBool("PowerfulUI.SelectableEditor.navigationFoldout", false); set => EditorPrefs.SetBool("PowerfulUI.SelectableEditor.navigationFoldout", value); }
        public bool eventsFoldout { get => EditorPrefs.GetBool("PowerfulUI.SelectableEditor.eventsFoldout", false); set => EditorPrefs.SetBool("PowerfulUI.SelectableEditor.eventsFoldout", value); }
        
        protected SerializedProperty m_ScriptProperty;
        protected SerializedProperty m_InteractableProperty;
        
        protected SerializedProperty m_TargetGraphicProperty;
        protected SerializedProperty m_TransitionProperty;
        protected SerializedProperty m_ColorBlockProperty;
        protected SerializedProperty m_SpriteStateProperty;
        protected SerializedProperty m_AnimTriggerProperty;
        protected AnimBool m_ShowColorTint = new AnimBool();
        protected AnimBool m_ShowSpriteTrasition = new AnimBool();
        protected AnimBool m_ShowAnimTransition  = new AnimBool();
        
        protected SerializedProperty m_NavigationProperty;
        protected GUIContent m_VisualizeNavigation = EditorGUIUtility.TrTextContent("Visualize", "Show navigation flows between selectable UI elements.");
        
        protected SerializedProperty m_OnClickProperty;
        protected SerializedProperty m_EnableLongPressProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_ScriptProperty = serializedObject.FindProperty("m_Script");
            m_InteractableProperty  = serializedObject.FindProperty("m_Interactable");
            
            m_TargetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
            m_TransitionProperty    = serializedObject.FindProperty("m_Transition");
            m_ColorBlockProperty    = serializedObject.FindProperty("m_Colors");
            m_SpriteStateProperty   = serializedObject.FindProperty("m_SpriteState");
            m_AnimTriggerProperty   = serializedObject.FindProperty("m_AnimationTriggers");

            var trans = (UnityEngine.UI.Selectable.Transition)m_TransitionProperty.enumValueIndex;
            m_ShowColorTint.value       = (trans == UnityEngine.UI.Selectable.Transition.ColorTint);
            m_ShowSpriteTrasition.value = (trans == UnityEngine.UI.Selectable.Transition.SpriteSwap);
            m_ShowAnimTransition.value  = (trans == UnityEngine.UI.Selectable.Transition.Animation);

            m_ShowColorTint.valueChanged.AddListener(Repaint);
            m_ShowSpriteTrasition.valueChanged.AddListener(Repaint);
            
            m_NavigationProperty    = serializedObject.FindProperty("m_Navigation");
            
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_EnableLongPressProperty = serializedObject.FindProperty("m_EnableLongPress");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(m_ScriptProperty);
            EditorGUILayout.PropertyField(m_InteractableProperty);
            
            transitionFoldout = EditorGUILayout.Foldout(transitionFoldout, "Transition", EditorStyles.foldoutHeader);
            if (transitionFoldout == true)
            {
                EditorGUI.indentLevel ++;
                OnInspectorGUI_Transition();
                EditorGUI.indentLevel --;
            }
            
            navigationFoldout = EditorGUILayout.Foldout(navigationFoldout, "Navigation", EditorStyles.foldoutHeader);
            if (navigationFoldout == true)
            {
                EditorGUI.indentLevel ++;
                OnInspectorGUI_Navigation();
                EditorGUI.indentLevel --;
            }
            
            eventsFoldout = EditorGUILayout.Foldout(eventsFoldout, "Events", EditorStyles.foldoutHeader);
            if (eventsFoldout == true)
            {
                EditorGUI.indentLevel ++;
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
                EditorGUI.indentLevel --;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void OnInspectorGUI_Transition()
        {
            var trans = (UnityEngine.UI.Selectable.Transition)m_TransitionProperty.enumValueIndex;

            var graphic = m_TargetGraphicProperty.objectReferenceValue as Graphic;
            if (graphic == null)
                graphic = (target as Selectable).GetComponent<Graphic>();

            var animator = (target as Selectable).GetComponent<Animator>();
            m_ShowColorTint.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == UnityEngine.UI.Selectable.Transition.ColorTint);
            m_ShowSpriteTrasition.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == UnityEngine.UI.Selectable.Transition.SpriteSwap);
            m_ShowAnimTransition.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == UnityEngine.UI.Selectable.Transition.Animation);

            EditorGUILayout.PropertyField(m_TransitionProperty);

            ++EditorGUI.indentLevel;
            {
                if (trans == Selectable.Transition.ColorTint || trans == UnityEngine.UI.Selectable.Transition.SpriteSwap)
                {
                    EditorGUILayout.PropertyField(m_TargetGraphicProperty);
                }

                switch (trans)
                {
                    case Selectable.Transition.ColorTint:
                        if (graphic == null)
                            EditorGUILayout.HelpBox("You must have a Graphic target in order to use a color transition.", MessageType.Warning);
                        break;

                    case Selectable.Transition.SpriteSwap:
                        if (graphic as Image == null)
                            EditorGUILayout.HelpBox("You must have a Image target in order to use a sprite swap transition.", MessageType.Warning);
                        break;
                }

                if (EditorGUILayout.BeginFadeGroup(m_ShowColorTint.faded))
                {
                    EditorGUILayout.PropertyField(m_ColorBlockProperty);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_ShowSpriteTrasition.faded))
                {
                    EditorGUILayout.PropertyField(m_SpriteStateProperty);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_ShowAnimTransition.faded))
                {
                    EditorGUILayout.PropertyField(m_AnimTriggerProperty);

                    if (animator == null || animator.runtimeAnimatorController == null)
                    {
                        Rect buttonRect = EditorGUILayout.GetControlRect();
                        buttonRect.xMin += EditorGUIUtility.labelWidth;
                        if (GUI.Button(buttonRect, "Auto Generate Animation", EditorStyles.miniButton))
                        {
                            // var controller = GenerateSelectableAnimatorContoller((target as Selectable).animationTriggers, target as Selectable);
                            var genAnimatorMethodInfo = typeof(UnityEditor.UI.SelectableEditor).GetMethod("GenerateSelectableAnimatorContoller", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            var controller = (UnityEditor.Animations.AnimatorController)genAnimatorMethodInfo.Invoke(null, new object[] { (target as Selectable).animationTriggers, target as Selectable });
                            if (controller != null)
                            {
                                if (animator == null)
                                    animator = (target as Selectable).gameObject.AddComponent<Animator>();

                                UnityEditor.Animations.AnimatorController.SetAnimatorController(animator, controller);
                            }
                        }
                    }
                }
                EditorGUILayout.EndFadeGroup();
            }
            --EditorGUI.indentLevel;
        }

        public virtual void OnInspectorGUI_Navigation()
        {
            EditorGUILayout.PropertyField(m_NavigationProperty);

            var showNavigationFieldInfo = typeof(UnityEditor.UI.SelectableEditor).GetField("s_ShowNavigation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var showNavigation = (bool)showNavigationFieldInfo.GetValue(null);
            
            EditorGUI.BeginChangeCheck();
            Rect toggleRect = EditorGUILayout.GetControlRect();
            toggleRect.xMin += EditorGUIUtility.labelWidth;
            showNavigation = GUI.Toggle(toggleRect, showNavigation, m_VisualizeNavigation, EditorStyles.miniButton);
            showNavigationFieldInfo.SetValue(null, showNavigation);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("SelectableEditor.ShowNavigation", showNavigation);
                SceneView.RepaintAll();
            }
        }
    }
}
