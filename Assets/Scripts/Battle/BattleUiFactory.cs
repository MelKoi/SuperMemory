using UnityEngine;
using UnityEngine.UI;

static class BattleUiFactory
{
    public static RectTransform Create(string name, Transform parent)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = 5;
        gameObject.hideFlags = HideFlags.DontSave;
        var rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchoredPosition3D = Vector3.zero;
        return rect;
    }

    public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    public static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    public static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
    {
        var rect = Create(name, parent);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = LabelFont();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(0.96f, 0.97f, 0.98f, 1f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    static Font LabelFont()
    {
        var font = ChineseFonts.Legacy;
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return font;
    }
}
