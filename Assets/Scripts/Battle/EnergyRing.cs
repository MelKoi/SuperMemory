using UnityEngine;
using UnityEngine.UI;

public sealed class EnergyRing : MaskableGraphic
{
    private int current;
    private int maximum = BattleVitals.EnergyCapacity;
    private float thickness = 14f;
    private Color trackColor = new Color(0.62f, 0.48f, 0.16f, 1f);
    private Color valueColor = new Color(0.98f, 0.84f, 0.32f, 1f);

    public void SetEnergy(int value, int capacity)
    {
        current = Mathf.Max(0, value);
        maximum = Mathf.Max(1, capacity);
        SetVerticesDirty();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        raycastTarget = false;
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        var rect = GetPixelAdjustedRect();
        var outer = Mathf.Min(rect.width * 0.5f, rect.height);
        var inner = outer - thickness;
        if (outer <= 1f || inner <= 1f)
        {
            return;
        }

        var center = new Vector2(rect.center.x, rect.yMin);
        AddArc(vertexHelper, center, inner, outer, Mathf.PI, 0f, 40, trackColor);

        var fill = maximum <= 0 ? 0f : Mathf.Clamp01(current / (float)maximum);
        if (fill <= 0.001f)
        {
            return;
        }

        var end = Mathf.Lerp(Mathf.PI, 0f, fill);
        var segments = Mathf.Max(2, Mathf.RoundToInt(40f * fill));
        AddArc(vertexHelper, center, inner + 1f, outer - 1f, Mathf.PI, end, segments, valueColor);
    }

    private static void AddArc(
        VertexHelper vertexHelper,
        Vector2 center,
        float inner,
        float outer,
        float startAngle,
        float endAngle,
        int segments,
        Color color)
    {
        var span = endAngle - startAngle;
        for (var i = 0; i < segments; i++)
        {
            var t0 = startAngle + span * (i / (float)segments);
            var t1 = startAngle + span * ((i + 1) / (float)segments);
            var direction0 = new Vector2(Mathf.Cos(t0), Mathf.Sin(t0));
            var direction1 = new Vector2(Mathf.Cos(t1), Mathf.Sin(t1));
            var index = vertexHelper.currentVertCount;
            vertexHelper.AddVert(center + direction0 * outer, color, Vector2.zero);
            vertexHelper.AddVert(center + direction1 * outer, color, Vector2.zero);
            vertexHelper.AddVert(center + direction1 * inner, color, Vector2.zero);
            vertexHelper.AddVert(center + direction0 * inner, color, Vector2.zero);
            vertexHelper.AddTriangle(index, index + 1, index + 2);
            vertexHelper.AddTriangle(index, index + 2, index + 3);
        }
    }
}
