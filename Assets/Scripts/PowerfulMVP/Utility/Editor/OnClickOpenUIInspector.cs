using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PowerfulMVP
{
    [CustomEditor(typeof(OnClickOpenUI))]
    public class OnClickOpenUIInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets.Length == 1)
            {
                var script = (OnClickOpenUI)target;
                if (script.Cache())
                    EditorGUILayout.HelpBox("This UI type can be used.", MessageType.Info, true);
                else
                    EditorGUILayout.HelpBox("This UI type cannot be used.", MessageType.Error, true);
            }
        }
    }
}
