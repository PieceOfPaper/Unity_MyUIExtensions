using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    public class OpenHandlerRegistEditor : EditorWindow
    {
        public static void Open(OpenHandler target)
        {
            var editor = CreateWindow<OpenHandlerRegistEditor>("Open Handler Regist Editor");
            editor.target = target;
        }
        
        
        public OpenHandler target;
        [FormerlySerializedAs("showOnlyHasAnimations")] public bool showOnlyAnims = false;
        public Transform selectObj;
        public Vector2 hierarchyScroll;
        public Vector2 inspectorScroll;

        private Component[] m_CachedComponents = null;
        private Dictionary<Object, Editor> m_CachedEditors = new Dictionary<Object, Editor>();
        private Dictionary<Object, bool> m_CachedEditorFoldouts = new Dictionary<Object, bool>();
        
        private void OnGUI()
        {
            target = (OpenHandler)EditorGUILayout.ObjectField("Target", target, typeof(OpenHandler), true);
            
            EditorGUI.BeginDisabledGroup(target == null);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(200f));
            showOnlyAnims = EditorGUILayout.Toggle("Show Only Anims", showOnlyAnims);
            hierarchyScroll = EditorGUILayout.BeginScrollView(hierarchyScroll);
            OnGUI_Hierarchy();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
            OnGUI_Inspector();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        private void OnGUI_Hierarchy()
        {
            if (target == null)
            {
                selectObj = null;
                hierarchyScroll = Vector2.zero;
                return;
            }

            OnGUI_HierarchyRecusively(target.transform, string.Empty, 0);
        }

        private void OnGUI_HierarchyRecusively(Transform transform, string hierarchiedName, int depth)
        {
            var components = transform.GetComponents<Component>();
            var hasAnim = System.Array.Exists(components, _ => _ is Animator);
            
#if DOTWEEN
            if (hasAnim == false)
                hasAnim = System.Array.Exists(components, _ => _ is DG.Tweening.DOTweenAnimation);
#endif

            var showName = string.IsNullOrWhiteSpace(hierarchiedName) ? transform.name : $"{hierarchiedName}/{transform.name}";
            if (hasAnim == true || showOnlyAnims == false)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(depth * 10f);
                if (GUILayout.Button(showName, GUILayout.ExpandWidth(false)))
                {
                    if (selectObj == transform)
                        selectObj = null;
                    else
                        selectObj = transform;
                    
                    inspectorScroll = Vector2.zero;
                    m_CachedComponents = null;
                    m_CachedEditors.Clear();
                    m_CachedEditorFoldouts.Clear();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            for (var i = 0; i < transform.childCount; i ++)
                OnGUI_HierarchyRecusively(transform.GetChild(i), showName, depth + 1);
        }

        private void OnGUI_Inspector()
        {
            if (selectObj == null)
            {
                inspectorScroll = Vector2.zero;
                m_CachedComponents = null;
                m_CachedEditors.Clear();
                m_CachedEditorFoldouts.Clear();
                return;
            }

            if (m_CachedComponents == null)
                m_CachedComponents = selectObj.GetComponents<Component>();

            for (int i = 0; i < m_CachedComponents.Length; i ++)
            {
                var component = m_CachedComponents[i];
                if (component == null) continue;

                if (m_CachedEditorFoldouts.ContainsKey(component) == false)
                    m_CachedEditorFoldouts.Add(component, false);
                m_CachedEditorFoldouts[component] = EditorGUILayout.BeginFoldoutHeaderGroup(m_CachedEditorFoldouts[component], component.GetType().Name);
                
                if (component is Animator animator)
                {
                    if (target.animator == animator)
                    {
                        if (GUILayout.Button("Unregist"))
                        {
                            target.animator = null;
                            EditorUtility.SetDirty(target);
                        }
                    }
                    else if (target.animator == null)
                    {
                        if (GUILayout.Button("Regist"))
                        {
                            target.animator = animator;
                            EditorUtility.SetDirty(target);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Change"))
                        {
                            target.animator = animator;
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
#if DOTWEEN
                else if (component is DG.Tweening.DOTweenAnimation doTweenAnimation)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (System.Array.Exists(target.doTweenAnimOpen, _ => _ == doTweenAnimation))
                    {
                        if (GUILayout.Button("Unregist Open"))
                        {
                            var list = new List<DG.Tweening.DOTweenAnimation>(target.doTweenAnimOpen);
                            list.Remove(doTweenAnimation);
                            target.doTweenAnimOpen = list.ToArray();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Regist Open"))
                        {
                            var list = new List<DG.Tweening.DOTweenAnimation>(target.doTweenAnimOpen);
                            list.Add(doTweenAnimation);
                            target.doTweenAnimOpen = list.ToArray();
                        }
                    }
                    if (System.Array.Exists(target.doTweenAnimClose, _ => _ == doTweenAnimation))
                    {
                        if (GUILayout.Button("Unregist Close"))
                        {
                            var list = new List<DG.Tweening.DOTweenAnimation>(target.doTweenAnimClose);
                            list.Remove(doTweenAnimation);
                            target.doTweenAnimClose = list.ToArray();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Regist Close"))
                        {
                            var list = new List<DG.Tweening.DOTweenAnimation>(target.doTweenAnimClose);
                            list.Add(doTweenAnimation);
                            target.doTweenAnimClose = list.ToArray();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
#endif
                
                if (m_CachedEditorFoldouts[component] == true)
                    GetEditor(component).OnInspectorGUI();
                
                EditorGUILayout.EndFoldoutHeaderGroup();
                GUILayout.Box(" ", GUILayout.ExpandWidth(true), GUILayout.Height(10f));
            }
        }

        private Editor GetEditor(Object obj)
        {
            if (m_CachedEditors.ContainsKey(obj) == false)
                m_CachedEditors.Add(obj, Editor.CreateEditor(obj));
            return m_CachedEditors[obj];
        }
    }
}
