using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace UnityEngine.UI
{
    [CustomEditor(typeof(OpenHandler))]
    [CanEditMultipleObjects]
    public class OpenHandlerEditor : Editor
    {
        private SerializedProperty m_Script;
        
        private SerializedProperty m_Animator;
        private SerializedProperty m_AnimatorOpenState;
        private SerializedProperty m_AnimatorCloseState;
        private SerializedProperty m_AnimatorLayer;
        
#if DOTWEEN
        private SerializedProperty m_DoTweenAnimOpen;
        private SerializedProperty m_DoTweenAnimClose;
#endif
        
        private SerializedProperty m_MinimumDuration;
        private SerializedProperty m_MinimumUnscaledDuration;
        private SerializedProperty m_AutoDisableAnimator;
        private SerializedProperty m_OpenOnStart;
        private SerializedProperty m_ControlActive;
        
        private SerializedProperty m_OnChangeState;

        private List<string> m_AnimatorLayerList = new List<string>();
        private List<string> m_AnimatorStateList = new List<string>();
        
        private void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            
            m_Animator = serializedObject.FindProperty("m_Animator");
            m_AnimatorOpenState = serializedObject.FindProperty("m_AnimatorOpenState");
            m_AnimatorCloseState = serializedObject.FindProperty("m_AnimatorCloseState");
            m_AnimatorLayer = serializedObject.FindProperty("m_AnimatorLayer");
            
#if DOTWEEN
            m_DoTweenAnimOpen = serializedObject.FindProperty("m_DoTweenAnimOpen");
            m_DoTweenAnimClose = serializedObject.FindProperty("m_DoTweenAnimClose");
#endif
            
            m_MinimumDuration = serializedObject.FindProperty("minimumDuration");
            m_MinimumUnscaledDuration = serializedObject.FindProperty("minimumUnscaledDuration");
            m_AutoDisableAnimator = serializedObject.FindProperty("autoDisableAnimator");
            m_OpenOnStart = serializedObject.FindProperty("openOnStart");
            m_ControlActive = serializedObject.FindProperty("controlActive");
            
            m_OnChangeState = serializedObject.FindProperty("onChangeState");
        }

        public override void OnInspectorGUI()
        {
            var openHandler = target as OpenHandler;
            
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(m_Script);

            if (targets.Length == 1)
            {
                EditorGUILayout.Space();
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.EnumPopup("State", openHandler.state);
            }
            
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animator", EditorStyles.boldLabel);
            EditorGUI.indentLevel ++;
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode == true))
                EditorGUILayout.PropertyField(m_Animator);
            using (new EditorGUI.DisabledScope((m_Animator.hasMultipleDifferentValues == false && m_Animator.objectReferenceValue == null) || EditorApplication.isPlayingOrWillChangePlaymode == true))
            {
                var animator = m_Animator.objectReferenceValue as Animator;
                if (targets.Length > 1 || 
                    (m_Animator.hasMultipleDifferentValues == false && m_Animator.objectReferenceValue == null) || 
                    animator.runtimeAnimatorController == null)
                {
                    EditorGUILayout.PropertyField(m_AnimatorLayer, new GUIContent("Layer"));
                    EditorGUILayout.PropertyField(m_AnimatorOpenState, new GUIContent("Open State"));
                    EditorGUILayout.PropertyField(m_AnimatorCloseState, new GUIContent("Close State"));
                }
                else
                {
                    var controller = openHandler.animator.runtimeAnimatorController as AnimatorController;
                    
                    m_AnimatorStateList.Clear();
                    m_AnimatorLayerList.AddRange(controller.layers.Select(_ => _.name));
                    var animatorLayers = m_AnimatorLayerList.ToArray();
                    var layerIndex = System.Array.IndexOf(animatorLayers, openHandler.animatorLayer);
                    var newLayerIndex = EditorGUILayout.Popup("Layer", layerIndex, animatorLayers);
                    if (layerIndex != newLayerIndex)
                        m_AnimatorLayer.stringValue = newLayerIndex < 0 || newLayerIndex >= animatorLayers.Length ? string.Empty : animatorLayers[newLayerIndex];
                    
                    m_AnimatorLayerList.Clear();
                    if (newLayerIndex >= 0 && newLayerIndex < controller.layers.Length)
                        m_AnimatorStateList.AddRange(controller.layers[newLayerIndex].stateMachine.states.Select(_ => _.state.name));
                    
                    var animatorStates = m_AnimatorStateList.ToArray();
                    var openStateIndex = System.Array.IndexOf(animatorStates, openHandler.animatorOpenState);
                    var newOpenStateIndex = EditorGUILayout.Popup("Open State", openStateIndex, animatorStates);
                    if (openStateIndex != newOpenStateIndex)
                        m_AnimatorOpenState.stringValue = newOpenStateIndex < 0 || newOpenStateIndex >= animatorStates.Length ? string.Empty : animatorStates[newOpenStateIndex];
                    var closeStateIndex = System.Array.IndexOf(animatorStates, openHandler.animatorCloseState);
                    var newCloseStateIndex = EditorGUILayout.Popup("Close State", closeStateIndex, animatorStates);
                    if (closeStateIndex != newCloseStateIndex)
                        m_AnimatorCloseState.stringValue = newCloseStateIndex < 0 || newCloseStateIndex >= animatorStates.Length ? string.Empty : animatorStates[newCloseStateIndex];
                }
            }
            EditorGUI.indentLevel --;
            
#if DOTWEEN
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("DOTweens", EditorStyles.boldLabel);
            EditorGUI.indentLevel ++;
            EditorGUILayout.PropertyField(m_DoTweenAnimOpen, new GUIContent("Open"));
            EditorGUILayout.PropertyField(m_DoTweenAnimClose, new GUIContent("Close"));
            EditorGUI.indentLevel --;
#endif
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel ++;
            EditorGUILayout.PropertyField(m_MinimumDuration, new GUIContent("Min Duration"));
            EditorGUILayout.PropertyField(m_MinimumUnscaledDuration, new GUIContent("Min Unscaled Duration"));
            EditorGUILayout.PropertyField(m_AutoDisableAnimator);
            EditorGUILayout.PropertyField(m_OpenOnStart);
            EditorGUILayout.PropertyField(m_ControlActive);
            EditorGUI.indentLevel --;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUI.indentLevel ++;
            EditorGUILayout.PropertyField(m_OnChangeState);
            EditorGUI.indentLevel --;
            
            serializedObject.ApplyModifiedProperties();
        }
    }

}