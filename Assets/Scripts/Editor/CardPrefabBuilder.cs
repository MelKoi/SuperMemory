using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CardPrefabBuilder
{
    const string Folder = "Assets/Prefabs/Cards";
    const string FontPath = "Assets/Resources/Fonts/NotoSansSC-Regular SDF.asset";

    public static void Create()
    {
        Directory.CreateDirectory(Folder);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            throw new FileNotFoundException("找不到中文字体：" + FontPath);
        }

        Build(font, "AttackCard", "攻击牌", CardFaceKind.Attack, "攻击牌", "下一次攻击时结算。", "2");
        Build(font, "ActionCard", "行动牌", CardFaceKind.Action, "行动牌", "使用后立即结算。", "2");
        Build(font, "BuffCard", "加成牌", CardFaceKind.Buff, "加成牌", "放在场上，读数归零后弃置。", "3");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void Build(TMP_FontAsset font, string assetName, string objectName, CardFaceKind kind, string nameText, string effectText, string costText)
    {
        Color backColor = CardBackColors.For(kind);
        GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.sizeDelta = new Vector2(240f, 348f);

        Image frame = root.GetComponent<Image>();
        frame.sprite = LoadSprite("UI/Skin/UISprite.psd");
        frame.type = Image.Type.Sliced;
        frame.color = backColor;
        frame.raycastTarget = true;

        Image back = CreateImage("卡背", root.transform, frame.sprite, backColor, Image.Type.Sliced);
        Stretch(back.rectTransform, 8f);
        TextMeshProUGUI backLabel = CreateText("类别", back.transform, font, CardView.KindLabel(kind), 42, TextAlignmentOptions.Center, new Color(0.96f, 0.94f, 0.90f, 0.92f));
        Stretch(backLabel.rectTransform, 24f);

        GameObject front = new GameObject("正面", typeof(RectTransform));
        front.transform.SetParent(root.transform, false);
        Stretch((RectTransform)front.transform, 0f);

        Image face = CreateImage("卡面", front.transform, frame.sprite, FaceColor(kind), Image.Type.Sliced);
        SetAnchors(face.rectTransform, new Vector2(0f, 0.58f), Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, -10f));

        Image plate = CreateImage("文字底", front.transform, frame.sprite, new Color(0.95f, 0.94f, 0.91f, 1f), Image.Type.Sliced);
        SetAnchors(plate.rectTransform, Vector2.zero, new Vector2(1f, 0.58f), new Vector2(10f, 8f), new Vector2(-10f, 0f));
        plate.raycastTarget = false;

        TextMeshProUGUI cardName = CreateText("卡名", front.transform, font, nameText, 26, TextAlignmentOptions.Center, new Color(0.12f, 0.12f, 0.14f, 1f));
        SetAnchors(cardName.rectTransform, new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.56f), Vector2.zero, Vector2.zero);
        Fit(cardName, 14f, 26f, VerticalAlignmentOptions.Middle);

        TextMeshProUGUI effect = CreateText("卡牌效果", front.transform, font, effectText, 18, TextAlignmentOptions.Top, new Color(0.20f, 0.20f, 0.22f, 1f));
        SetAnchors(effect.rectTransform, new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.35f), Vector2.zero, Vector2.zero);
        Fit(effect, 11f, 18f, VerticalAlignmentOptions.Top);

        Image subtypeBar = CreateImage("小类", front.transform, frame.sprite, new Color(0.16f, 0.16f, 0.18f, 1f), Image.Type.Sliced);
        SetAnchors(subtypeBar.rectTransform, Vector2.zero, new Vector2(1f, 0.15f), new Vector2(14f, 8f), new Vector2(-14f, -2f));
        TextMeshProUGUI subtypeLabel = CreateText("名称", subtypeBar.transform, font, "无", 16, TextAlignmentOptions.Center, new Color(0.96f, 0.94f, 0.90f, 1f));
        Stretch(subtypeLabel.rectTransform, 2f);
        Fit(subtypeLabel, 11f, 16f, VerticalAlignmentOptions.Middle);

        Image costBadge = CreateImage("卡牌消耗", front.transform, LoadSprite("UI/Skin/Knob.psd"), new Color(0.12f, 0.12f, 0.14f, 1f), Image.Type.Simple);
        RectTransform costRect = costBadge.rectTransform;
        costRect.anchorMin = new Vector2(0f, 1f);
        costRect.anchorMax = new Vector2(0f, 1f);
        costRect.pivot = new Vector2(0f, 1f);
        costRect.anchoredPosition = new Vector2(18f, -18f);
        costRect.sizeDelta = new Vector2(54f, 54f);
        TextMeshProUGUI cost = CreateText("数值", costBadge.transform, font, costText, 26, TextAlignmentOptions.Center, new Color(0.97f, 0.95f, 0.90f, 1f));
        Stretch(cost.rectTransform, 4f);

        CardView view = root.AddComponent<CardView>();
        SerializedObject serialized = new SerializedObject(view);
        serialized.FindProperty("faceKind").enumValueIndex = (int)kind;
        serialized.FindProperty("frame").objectReferenceValue = frame;
        serialized.FindProperty("face").objectReferenceValue = face;
        serialized.FindProperty("back").objectReferenceValue = back;
        serialized.FindProperty("backLabel").objectReferenceValue = backLabel;
        serialized.FindProperty("cardName").objectReferenceValue = cardName;
        serialized.FindProperty("effect").objectReferenceValue = effect;
        serialized.FindProperty("cost").objectReferenceValue = cost;
        serialized.FindProperty("subtype").objectReferenceValue = subtypeLabel;
        serialized.FindProperty("front").objectReferenceValue = front;
        serialized.FindProperty("faceUp").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        back.gameObject.SetActive(false);
        SetLayer(root);

        string path = Folder + "/" + assetName + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        if (prefab != null)
        {
            prefab.name = objectName;
        }

        Object.DestroyImmediate(root);
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Image.Type type)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, TMP_FontAsset font, string content, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    static void Fit(TMP_Text text, float minSize, float maxSize, VerticalAlignmentOptions alignment)
    {
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.verticalAlignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.useMaxVisibleDescender = false;
    }

    static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
        if (sprite == null)
        {
            throw new FileNotFoundException("找不到内置图片：" + path);
        }

        return sprite;
    }

    static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    static void SetLayer(GameObject gameObject)
    {
        gameObject.layer = 5;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            SetLayer(gameObject.transform.GetChild(i).gameObject);
        }
    }

    static Color FaceColor(CardFaceKind kind)
    {
        Color back = CardBackColors.For(kind);
        return Color.Lerp(back, Color.white, 0.55f);
    }
}
