using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class HandZone : MonoBehaviour
{
    [SerializeField] private BattleSide side;

    public BattleSide Side => side;
    public RectTransform Area => (RectTransform)transform;

    public void Initialize(BattleSide owner)
    {
        side = owner;
    }

    public void Present(IReadOnlyList<string> names)
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        if (names == null)
        {
            return;
        }

        for (var i = 0; i < names.Count; i++)
        {
            var card = BattleUiFactory.Create("Card", transform);
            card.anchorMin = new Vector2(0f, 0f);
            card.anchorMax = new Vector2(0f, 1f);
            card.pivot = new Vector2(0f, 0.5f);
            card.anchoredPosition = new Vector2(8f + i * 168f, 0f);
            card.sizeDelta = new Vector2(156f, -12f);
            var image = card.gameObject.AddComponent<Image>();
            image.sprite = BattleUiSprites.Square;
            image.color = new Color(0.94f, 0.95f, 0.96f, 1f);
            var label = BattleUiFactory.CreateText("Name", card, names[i] ?? string.Empty, 22, TextAnchor.MiddleCenter);
            label.color = new Color(0.16f, 0.16f, 0.18f, 1f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            BattleUiFactory.Stretch(label.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f));
        }
    }

    public Rect ScreenRect
    {
        get
        {
            var corners = new Vector3[4];
            Area.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        var corners = new Vector3[4];
        Area.GetWorldCorners(corners);
        Gizmos.color = side == BattleSide.Player
            ? new Color(0.25f, 0.55f, 1f, 0.9f)
            : new Color(1f, 0.35f, 0.3f, 0.9f);
        for (var i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
}
