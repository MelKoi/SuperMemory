using UnityEngine;

public sealed class HandZone : MonoBehaviour
{
    [SerializeField] private BattleSide side;

    public BattleSide Side => side;
    public RectTransform Area => (RectTransform)transform;

    public void Initialize(BattleSide owner)
    {
        side = owner;
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
