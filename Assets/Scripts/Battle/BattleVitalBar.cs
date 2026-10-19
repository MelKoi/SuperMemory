using UnityEngine;
using UnityEngine.UI;

public sealed class BattleVitalBar : MonoBehaviour
{
    private Image fill;

    public void Initialize(Color fillColor, bool fillFromRight)
    {
        var background = gameObject.AddComponent<Image>();
        background.sprite = BattleUiSprites.Square;
        background.color = new Color(0.12f, 0.08f, 0.08f, 0.92f);
        background.raycastTarget = false;

        var fillRect = BattleUiFactory.Create("Fill", transform);
        BattleUiFactory.Stretch(fillRect, Vector2.zero, Vector2.one);
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);

        fill = fillRect.gameObject.AddComponent<Image>();
        fill.sprite = BattleUiSprites.Square;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = fillFromRight ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        fill.color = fillColor;
        fill.raycastTarget = false;
    }

    public void SetValue(int current, int maximum)
    {
        if (fill == null)
        {
            return;
        }

        fill.fillAmount = maximum <= 0 ? 0f : Mathf.Clamp01(current / (float)maximum);
    }
}
