using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArBlokEvren.UI
{
    /// <summary>Small helpers to build a legacy-uGUI interface entirely from code.</summary>
    public static class UIFactory
    {
        public static Font DefaultFont()
        {
            // Built-in font shipped with the editor/runtime (Unity 2022.x).
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        public static Text CreateText(Transform parent, string content, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = DefaultFont();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(Transform parent, string label, Color color, Action onClick, out Text labelText)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;

            var button = go.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            labelText = CreateText(go.transform, label, 30, TextAnchor.MiddleCenter);
            StretchToParent(labelText.rectTransform);
            return button;
        }

        public static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
