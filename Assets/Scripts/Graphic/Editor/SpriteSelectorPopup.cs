using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;

namespace UnityEngine.U2D
{
    public class SpriteSelectorPopup : EditorWindow
    {
        private static Sprite[] s_SpriteBuffer = new Sprite[1024];

        private string search;
        private SpriteAtlas spriteAtlas;
        private SerializedProperty spriteNameProperty;
        private Vector2 scrollPosition;

        public static void ShowWindow(SpriteAtlas spriteAtlas, SerializedProperty spriteNameProperty)
        {
            SpriteSelectorPopup window = GetWindow<SpriteSelectorPopup>();
            window.search = string.Empty;
            window.spriteAtlas = spriteAtlas;
            window.spriteNameProperty = spriteNameProperty;
            window.ShowPopup();
        }

        private void OnGUI()
        {
            search = EditorGUILayout.TextField(search);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (spriteAtlas != null)
            {
                spriteAtlas.GetSprites(s_SpriteBuffer);
                foreach (var sprite in s_SpriteBuffer)
                {
                    if (sprite == null) continue;
                    if (string.IsNullOrWhiteSpace(search) == false && sprite.name.Contains(search) == false) continue;

                    var sptireName = sprite.name.Replace("(Clone)", "");
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(AssetPreview.GetAssetPreview(sprite), GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        spriteNameProperty.stringValue = sptireName;
                        spriteNameProperty.serializedObject.ApplyModifiedProperties();
                        Close();
                    }
                    GUILayout.Label(sptireName);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

}