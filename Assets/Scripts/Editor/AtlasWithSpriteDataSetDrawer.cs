using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.U2D
{
    [CustomPropertyDrawer (typeof(AtlasWithSpriteDataSet))]
    public class AtlasWithSpriteDataSetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var atlasProperty = property.FindPropertyRelative("atlas");
            var spriteNameProperty = property.FindPropertyRelative("spriteName");
            var atlasRect = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            var spriteNameRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.PropertyField(atlasRect, atlasProperty, new GUIContent(string.Empty));
            
            // Get the SpriteAtlas from the SerializedProperty
            SpriteAtlas spriteAtlas = (SpriteAtlas)atlasProperty.objectReferenceValue;
            if (spriteAtlas != null)
            {
                if (GUI.Button(spriteNameRect, spriteNameProperty.stringValue))
                {
                    SpriteSelectorPopup.ShowWindow(spriteAtlas, spriteNameProperty);
                }
            }
            else
            {
                EditorGUI.PropertyField(spriteNameRect, spriteNameProperty, new GUIContent(string.Empty));
            }
            
            EditorGUI.EndProperty();
        }
    }
}
