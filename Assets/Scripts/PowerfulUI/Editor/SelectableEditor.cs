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
        
        //변수 이름 앞에 PowerfulUI_ 붙인 이유는,
        //UnityEditor.UI.SelectableEditor에 변수명과 중첩되서 어쩔 수 없이 붙였다.
        protected SerializedProperty m_PowerfulUI_ScriptProperty;
        protected SerializedProperty m_PowerfulUI_InteractableProperty;
        
        protected SerializedProperty m_PowerfulUI_TargetGraphicProperty;
        protected SerializedProperty m_PowerfulUI_TransitionProperty;
        protected SerializedProperty m_PowerfulUI_ColorBlockProperty;
        protected SerializedProperty m_PowerfulUI_SpriteStateProperty;
        protected SerializedProperty m_PowerfulUI_AnimTriggerProperty;
        protected AnimBool m_PowerfulUI_ShowColorTint = new AnimBool();
        protected AnimBool m_PowerfulUI_ShowSpriteTrasition = new AnimBool();
        protected AnimBool m_PowerfulUI_ShowAnimTransition  = new AnimBool();
        
        protected SerializedProperty m_PowerfulUI_NavigationProperty;
        protected GUIContent m_PowerfulUI_VisualizeNavigation = EditorGUIUtility.TrTextContent("Visualize", "Show navigation flows between selectable UI elements.");
        
        protected SerializedProperty m_PowerfulUI_OnClickProperty;
        protected SerializedProperty m_PowerfulUI_EnableLongPressProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_PowerfulUI_ScriptProperty = serializedObject.FindProperty("m_Script");
            m_PowerfulUI_InteractableProperty = serializedObject.FindProperty("m_Interactable");
            
            m_PowerfulUI_TargetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
            m_PowerfulUI_TransitionProperty = serializedObject.FindProperty("m_Transition");
            m_PowerfulUI_ColorBlockProperty = serializedObject.FindProperty("m_Colors");
            m_PowerfulUI_SpriteStateProperty = serializedObject.FindProperty("m_SpriteState");
            m_PowerfulUI_AnimTriggerProperty = serializedObject.FindProperty("m_AnimationTriggers");

            var trans = (UnityEngine.UI.Selectable.Transition)m_PowerfulUI_TransitionProperty.enumValueIndex;
            m_PowerfulUI_ShowColorTint.value = (trans == UnityEngine.UI.Selectable.Transition.ColorTint);
            m_PowerfulUI_ShowSpriteTrasition.value = (trans == UnityEngine.UI.Selectable.Transition.SpriteSwap);
            m_PowerfulUI_ShowAnimTransition.value = (trans == UnityEngine.UI.Selectable.Transition.Animation);

            m_PowerfulUI_ShowColorTint.valueChanged.AddListener(Repaint);
            m_PowerfulUI_ShowSpriteTrasition.valueChanged.AddListener(Repaint);
            
            m_PowerfulUI_NavigationProperty = serializedObject.FindProperty("m_Navigation");
            
            m_PowerfulUI_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_PowerfulUI_EnableLongPressProperty = serializedObject.FindProperty("m_EnableLongPress");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(m_PowerfulUI_ScriptProperty);
            EditorGUILayout.PropertyField(m_PowerfulUI_InteractableProperty);
            
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
                EditorGUILayout.PropertyField(m_PowerfulUI_OnClickProperty);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSubmitToClick"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EnableEventOnDisabled"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_PowerfulUI_EnableLongPressProperty);
                if (m_PowerfulUI_EnableLongPressProperty.hasMultipleDifferentValues || m_PowerfulUI_EnableLongPressProperty.boolValue == true)
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
            var trans = (UnityEngine.UI.Selectable.Transition)m_PowerfulUI_TransitionProperty.enumValueIndex;

            var graphic = m_PowerfulUI_TargetGraphicProperty.objectReferenceValue as Graphic;
            if (graphic == null)
                graphic = (target as Selectable).GetComponent<Graphic>();

            var animator = (target as Selectable).GetComponent<Animator>();
            m_PowerfulUI_ShowColorTint.target = (!m_PowerfulUI_TransitionProperty.hasMultipleDifferentValues && trans == UnityEngine.UI.Selectable.Transition.ColorTint);
            m_PowerfulUI_ShowSpriteTrasition.target = (!m_PowerfulUI_TransitionProperty.hasMultipleDifferentValues && trans == UnityEngine.UI.Selectable.Transition.SpriteSwap);
            m_PowerfulUI_ShowAnimTransition.target = (!m_PowerfulUI_TransitionProperty.hasMultipleDifferentValues && trans == UnityEngine.UI.Selectable.Transition.Animation);

            EditorGUILayout.PropertyField(m_PowerfulUI_TransitionProperty);

            ++EditorGUI.indentLevel;
            {
                if (trans == Selectable.Transition.ColorTint || trans == UnityEngine.UI.Selectable.Transition.SpriteSwap)
                {
                    EditorGUILayout.PropertyField(m_PowerfulUI_TargetGraphicProperty);
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

                if (EditorGUILayout.BeginFadeGroup(m_PowerfulUI_ShowColorTint.faded))
                {
                    EditorGUILayout.PropertyField(m_PowerfulUI_ColorBlockProperty);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_PowerfulUI_ShowSpriteTrasition.faded))
                {
                    EditorGUILayout.PropertyField(m_PowerfulUI_SpriteStateProperty);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_PowerfulUI_ShowAnimTransition.faded))
                {
                    EditorGUILayout.PropertyField(m_PowerfulUI_AnimTriggerProperty);

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
            EditorGUILayout.PropertyField(m_PowerfulUI_NavigationProperty);

            var showNavigationFieldInfo = typeof(UnityEditor.UI.SelectableEditor).GetField("s_ShowNavigation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var showNavigation = (bool)showNavigationFieldInfo.GetValue(null);
            
            EditorGUI.BeginChangeCheck();
            Rect toggleRect = EditorGUILayout.GetControlRect();
            toggleRect.xMin += EditorGUIUtility.labelWidth;
            showNavigation = GUI.Toggle(toggleRect, showNavigation, m_PowerfulUI_VisualizeNavigation, EditorStyles.miniButton);
            showNavigationFieldInfo.SetValue(null, showNavigation);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("SelectableEditor.ShowNavigation", showNavigation);
                SceneView.RepaintAll();
            }
        }
    }
}
