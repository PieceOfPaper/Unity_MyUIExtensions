using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace PowerfulUI
{
    public class MenuItems
    {
        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath = "UI/Skin/UIMask.psd";
        
        
        [MenuItem("GameObject/Powerful UI/Selectable", false, 0)]
        public static void CreateSelectable()
        {
            // Selectable
            var obj = new GameObject("Selectable");
            
            var rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 30);
            rectTransform.anchoredPosition = Vector2.zero;
            
            obj.AddComponent<CanvasRenderer>();
            
            var img = obj.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
            img.type = Image.Type.Sliced;
            
            var selectable = obj.AddComponent<Selectable>();

            
            // Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform);
            
            var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "Powerful!";
            text.fontSize = 24;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;

            var textRectTransform = textObj.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.anchoredPosition = Vector2.zero;


            if (Selection.activeGameObject != null)
                obj.transform.SetParent(Selection.activeGameObject.transform, false);

            Selection.activeGameObject = obj;
        }
    }
}