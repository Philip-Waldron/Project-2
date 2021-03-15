using TMPro;
using UnityEngine;

namespace XR_Prototyping.Scripts.Utilities.Extensions
{
    public static class TextMeshProExtension
    {
        public static TextMeshPro TextMeshPro(this GameObject parent, Vector2 sizeDelta, TMP_FontAsset font, HorizontalAlignmentOptions horizontalAlignment, VerticalAlignmentOptions verticalAlignment, float size = 0.01f, bool overlay = false)
        {
            TextMeshPro text = parent.AddComponent<TextMeshPro>();
            text.enableWordWrapping = false;
            text.isOverlay = overlay;
            text.rectTransform.sizeDelta = sizeDelta;
            text.font = font;
            text.fontSize = size;
            text.horizontalAlignment = horizontalAlignment;
            text.verticalAlignment = verticalAlignment;
            text.color = Color.black;
            return text;
        }
    }
}