using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 牌面外观。卡背颜色对应三种大类：攻击、行动、加成。
public enum CardFaceKind
{
    Attack = 0,
    Action = 1,
    Buff = 2,
}

public static class CardBackColors
{
    public static readonly Color Attack = new Color(0.77f, 0.38f, 0.24f, 1f);
    public static readonly Color Action = new Color(0.36f, 0.42f, 0.71f, 1f);
    public static readonly Color Buff = new Color(0.77f, 0.60f, 0.27f, 1f);

    public static Color For(CardFaceKind kind)
    {
        switch (kind)
        {
            case CardFaceKind.Attack:
                return Attack;
            case CardFaceKind.Buff:
                return Buff;
            default:
                return Action;
        }
    }

    public static CardFaceKind From(CardKind kind)
    {
        switch (kind)
        {
            case CardKind.Attack:
                return CardFaceKind.Attack;
            case CardKind.Buff:
                return CardFaceKind.Buff;
            default:
                return CardFaceKind.Action;
        }
    }
}

public sealed class CardView : MonoBehaviour
{
    [SerializeField] private CardFaceKind faceKind;
    [SerializeField] private Image frame;
    [SerializeField] private Image face;
    [SerializeField] private Image back;
    [SerializeField] private TMP_Text backLabel;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text effect;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text subtype;
    [SerializeField] private GameObject front;
    [SerializeField] private bool faceUp = true;

    public CardFaceKind FaceKind => faceKind;
    public bool FaceUp => faceUp;

    private void Awake()
    {
        ArrangeFront();
        ApplyFace();
    }

    private bool labelsNeedFit;
    private bool fittingLabels;

    private void OnRectTransformDimensionsChange()
    {
        if (!fittingLabels)
        {
            labelsNeedFit = true;
        }
    }

    private void LateUpdate()
    {
        if (!labelsNeedFit)
        {
            return;
        }

        labelsNeedFit = false;
        fittingLabels = true;
        FitLabels();
        fittingLabels = false;
    }

    private void OnValidate()
    {
        ApplyKindColor();
        ApplyFace();
    }

    public void Present(string nameText, string effectText, int costValue, int subtypeId)
    {
        Present(nameText, effectText, costValue.ToString(), subtypeId);
    }

    public void Present(string nameText, string effectText, string costText, int subtypeId)
    {
        Present(nameText, effectText, costText);
        SetSubtype(subtypeId);
    }

    public void Present(string nameText, string effectText, int costValue)
    {
        Present(nameText, effectText, costValue.ToString());
    }

    public void Present(string nameText, string effectText, string costText)
    {
        if (cardName != null)
        {
            cardName.text = nameText ?? string.Empty;
        }

        if (effect != null)
        {
            effect.text = effectText ?? string.Empty;
        }

        if (cost != null)
        {
            cost.text = string.IsNullOrEmpty(costText) ? "0" : costText;
        }

        labelsNeedFit = true;
    }

    // 手牌会把预制体收成大约 116×168。卡名栏如果仍按 28 号字排，一行中文会被裁没。
    private void ArrangeFront()
    {
        if (front == null)
        {
            return;
        }

        var plate = front.transform.Find("文字底") as RectTransform;
        if (plate != null)
        {
            Place(plate, 0f, 0f, 1f, 0.58f, new Vector2(10f, 8f), new Vector2(-10f, 0f));
        }

        if (face != null)
        {
            Place(face.rectTransform, 0f, 0.58f, 1f, 1f, new Vector2(10f, 0f), new Vector2(-10f, -10f));
        }

        Place(cardName, 0.06f, 0.42f, 0.94f, 0.56f, Vector2.zero, Vector2.zero);
        Place(effect, 0.06f, 0.16f, 0.94f, 0.41f, Vector2.zero, Vector2.zero);
        if (subtype != null && subtype.rectTransform.parent is RectTransform bar && bar.parent == front.transform)
        {
            Place(bar, 0f, 0f, 1f, 0.15f, new Vector2(14f, 8f), new Vector2(-14f, -2f));
            Place(subtype, 0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);
        }

        if (cardName != null)
        {
            cardName.transform.SetAsLastSibling();
        }
    }

    private void FitLabels()
    {
        Fit(cardName, 14f, 26f, VerticalAlignmentOptions.Middle);
        Fit(effect, 10f, 16f, VerticalAlignmentOptions.Top);
        Fit(subtype, 11f, 16f, VerticalAlignmentOptions.Middle);
    }

    private static void Place(TMP_Text text, float minX, float minY, float maxX, float maxY, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (text == null)
        {
            return;
        }

        Place(text.rectTransform, minX, minY, maxX, maxY, offsetMin, offsetMax);
    }

    private static void Place(RectTransform rect, float minX, float minY, float maxX, float maxY, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void Fit(TMP_Text text, float minSize, float maxSize, VerticalAlignmentOptions alignment)
    {
        if (text == null)
        {
            return;
        }

        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.verticalAlignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.useMaxVisibleDescender = false;
        text.ForceMeshUpdate();
    }

    public void SetSubtype(int subtypeId)
    {
        if (subtype != null)
        {
            subtype.text = SubtypeLabel(subtypeId);
        }
    }

    public void ApplyKind(CardKind kind)
    {
        ApplyKind(CardBackColors.From(kind));
    }

    public void ApplyKind(CardFaceKind kind)
    {
        faceKind = kind;
        ApplyKindColor();
    }

    public void SetFaceUp(bool value)
    {
        faceUp = value;
        ApplyFace();
    }

    private void ApplyKindColor()
    {
        Color color = CardBackColors.For(faceKind);
        if (frame != null)
        {
            frame.color = color;
        }

        if (back != null)
        {
            back.color = color;
        }

        if (backLabel != null)
        {
            backLabel.text = KindLabel(faceKind);
        }
    }

    private void ApplyFace()
    {
        if (front != null)
        {
            front.SetActive(faceUp);
        }

        if (back != null)
        {
            back.gameObject.SetActive(!faceUp);
        }
    }

    public static string KindLabel(CardFaceKind kind)
    {
        switch (kind)
        {
            case CardFaceKind.Attack:
                return "攻击";
            case CardFaceKind.Action:
                return "行动";
            case CardFaceKind.Buff:
                return "加成";
            default:
                return "行动";
        }
    }

    public static string EffectLine(CardDefinition card)
    {
        if (card == null)
        {
            return string.Empty;
        }

        return card.Description;
    }

    public static string SubtypeLabel(int subtypeId)
    {
        switch (subtypeId)
        {
            case CardSubtypes.Response:
                return "对应";
            case CardSubtypes.AllOut:
                return "全力";
            case CardSubtypes.Temporary:
                return "临时";
            case CardSubtypes.Restricted:
                return "限制";
            default:
                return "无";
        }
    }
}
