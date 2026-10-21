using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DefaultExecutionOrder(-50)]
public sealed class BattleBoardPerspective : MonoBehaviour
{
    [SerializeField] private float pitch = 60f;
    [SerializeField] private float viewScale = 0.9f;
    [SerializeField] private float cellSize = 0.92f;
    [SerializeField] private string frameName = "BoardFrame";
    [SerializeField] private int playerColumn = 1;
    [SerializeField] private int playerRow;
    [SerializeField] private int enemyColumn = 1;
    [SerializeField] private int enemyRow = 11;

    private const string LabelRootName = "BoardLabels";
    private int boardRows;
    private const float LabelCanvasScale = 0.01f;
    private static Mesh cylinderMesh;

    private Transform frame;
    private Font labelFont;

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    private void LateUpdate()
    {
        Apply();
    }

    public void Apply()
    {
        var scale = Mathf.Max(0.01f, viewScale);
        SetLocal(transform, Vector3.zero, Quaternion.Euler(pitch, 0f, 0f), new Vector3(scale, scale, scale));

        var cells = GetComponentsInChildren<BoardCell>(true);
        var columns = 1;
        var rows = 1;
        for (var i = 0; i < cells.Length; i++)
        {
            columns = Mathf.Max(columns, cells[i].Column + 1);
            rows = Mathf.Max(rows, cells[i].Row + 1);
        }

        var columnCenter = (columns - 1) * 0.5f;
        var rowCenter = (rows - 1) * 0.5f;
        for (var i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            var localPosition = new Vector3(
                (cell.Column - columnCenter) * cellSize,
                (cell.Row - rowCenter) * cellSize,
                0f);
            SetLocal(cell.transform, localPosition, Quaternion.identity, new Vector3(cellSize, cellSize, 1f));

            var renderer = cell.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = rows - cell.Row;
            }
        }

        boardRows = rows;
        if (frame == null)
        {
            var child = transform.Find(frameName);
            if (child != null)
            {
                frame = child;
            }
        }

        if (frame == null)
        {
            return;
        }

        var width = columns * cellSize;
        var height = rows * cellSize;
        SetLocal(frame, Vector3.zero, Quaternion.identity, new Vector3(width, height, 1f));
        SyncLabels(columns, rows);
        SyncPieces(columns, rows);
    }

    // 我方在近端，敌方在远端。前进增加行号，朝敌方走。
    public bool CanMovePlayer(int rowDelta)
    {
        if (rowDelta == 0 || boardRows <= 0)
        {
            return false;
        }

        var nextRow = playerRow + rowDelta;
        if (nextRow < 0 || nextRow >= boardRows)
        {
            return false;
        }

        if (playerColumn != enemyColumn)
        {
            return true;
        }

        if (enemyRow == nextRow)
        {
            return false;
        }

        if (rowDelta > 0 && enemyRow > playerRow && enemyRow < nextRow)
        {
            return false;
        }

        if (rowDelta < 0 && enemyRow < playerRow && enemyRow > nextRow)
        {
            return false;
        }

        return true;
    }

    public bool TryMovePlayer(int rowDelta)
    {
        if (!CanMovePlayer(rowDelta))
        {
            return false;
        }

        playerRow += rowDelta;
        Apply();
        return true;
    }

    private void SyncPieces(int columns, int rows)
    {
        PlacePiece("Unit_1B", playerColumn, playerRow, columns, rows, new Color(0.30f, 0.76f, 0.92f, 1f));
        PlacePiece("Unit_12B", enemyColumn, enemyRow, columns, rows, new Color(0.48f, 0.10f, 0.13f, 1f));
    }

    private void PlacePiece(string pieceName, int column, int row, int columns, int rows, Color color)
    {
        if (column < 0 || row < 0 || column >= columns || row >= rows)
        {
            return;
        }

        var piece = transform.Find(pieceName);
        if (piece == null)
        {
            var pieceObject = new GameObject(pieceName);
            pieceObject.hideFlags = HideFlags.DontSave;
            pieceObject.transform.SetParent(transform, false);
            var filter = pieceObject.AddComponent<MeshFilter>();
            filter.sharedMesh = CylinderMesh();
            var renderer = pieceObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = PieceMaterial(color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 20;
            piece = pieceObject.transform;
        }

        var diameter = cellSize * 0.68f;
        var height = cellSize * 0.42f;
        var columnCenter = (columns - 1) * 0.5f;
        var rowCenter = (rows - 1) * 0.5f;
        var center = new Vector3((column - columnCenter) * cellSize, (row - rowCenter) * cellSize, height * 0.5f + 0.02f);
        SetLocal(piece, center, Quaternion.Euler(90f, 0f, 0f), new Vector3(diameter, height * 0.5f, diameter));
    }

    private static Mesh CylinderMesh()
    {
        if (cylinderMesh != null)
        {
            return cylinderMesh;
        }

        // 与 CreatePrimitive 圆柱一致：半径 0.5、高 2。不附带碰撞体，OnValidate 里不能立刻删组件。
        const int segments = 24;
        const float radius = 0.5f;
        const float halfHeight = 1f;
        var vertices = new Vector3[(segments + 1) * 2 + segments * 2 + 2];
        var triangles = new int[segments * 4 * 3];
        var vertex = 0;
        var triangle = 0;

        for (var i = 0; i <= segments; i++)
        {
            var angle = i * Mathf.PI * 2f / segments;
            var x = Mathf.Cos(angle) * radius;
            var z = Mathf.Sin(angle) * radius;
            vertices[vertex++] = new Vector3(x, -halfHeight, z);
            vertices[vertex++] = new Vector3(x, halfHeight, z);
        }

        for (var i = 0; i < segments; i++)
        {
            var bottom = i * 2;
            var top = bottom + 1;
            var nextBottom = bottom + 2;
            var nextTop = bottom + 3;
            triangles[triangle++] = bottom;
            triangles[triangle++] = top;
            triangles[triangle++] = nextBottom;
            triangles[triangle++] = top;
            triangles[triangle++] = nextTop;
            triangles[triangle++] = nextBottom;
        }

        var bottomCenter = vertex;
        vertices[vertex++] = new Vector3(0f, -halfHeight, 0f);
        var topCenter = vertex;
        vertices[vertex++] = new Vector3(0f, halfHeight, 0f);
        var bottomRim = vertex;
        for (var i = 0; i < segments; i++)
        {
            var angle = i * Mathf.PI * 2f / segments;
            vertices[vertex++] = new Vector3(Mathf.Cos(angle) * radius, -halfHeight, Mathf.Sin(angle) * radius);
        }

        var topRim = vertex;
        for (var i = 0; i < segments; i++)
        {
            var angle = i * Mathf.PI * 2f / segments;
            vertices[vertex++] = new Vector3(Mathf.Cos(angle) * radius, halfHeight, Mathf.Sin(angle) * radius);
        }

        for (var i = 0; i < segments; i++)
        {
            var next = (i + 1) % segments;
            triangles[triangle++] = bottomCenter;
            triangles[triangle++] = bottomRim + next;
            triangles[triangle++] = bottomRim + i;
            triangles[triangle++] = topCenter;
            triangles[triangle++] = topRim + i;
            triangles[triangle++] = topRim + next;
        }

        cylinderMesh = new Mesh { name = "BattlePieceCylinder" };
        cylinderMesh.hideFlags = HideFlags.HideAndDontSave;
        cylinderMesh.vertices = vertices;
        cylinderMesh.triangles = triangles;
        cylinderMesh.RecalculateNormals();
        cylinderMesh.RecalculateBounds();
        return cylinderMesh;
    }

    private static Material PieceMaterial(Color color)
    {
        var shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        var material = new Material(shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        return material;
    }

    private void SyncLabels(int columns, int rows)
    {
        var font = LabelFont();
        if (font == null)
        {
            return;
        }

        var root = transform.Find(LabelRootName);
        if (root != null && root.childCount != columns + rows)
        {
            if (Application.isPlaying)
            {
                Destroy(root.gameObject);
            }
            else
            {
                DestroyImmediate(root.gameObject);
            }

            root = null;
        }

        if (root == null)
        {
            root = CreateLabelRoot();
            for (var row = 0; row < rows; row++)
            {
                CreateLabel(root, "Row" + (row + 1), (row + 1).ToString(), font);
            }

            for (var column = 0; column < columns; column++)
            {
                CreateLabel(root, "Column" + ColumnLetter(column), ColumnLetter(column), font);
            }
        }

        var outside = cellSize * 0.72f;
        var leftX = -columns * cellSize * 0.5f - outside;
        var bottomY = -rows * cellSize * 0.5f - outside;
        var rowCenter = (rows - 1) * 0.5f;
        var columnCenter = (columns - 1) * 0.5f;
        for (var row = 0; row < rows; row++)
        {
            var label = root.Find("Row" + (row + 1)) as RectTransform;
            if (label == null)
            {
                continue;
            }

            var y = (row - rowCenter) * cellSize;
            label.anchoredPosition = new Vector2(leftX / LabelCanvasScale, y / LabelCanvasScale);
        }

        for (var column = 0; column < columns; column++)
        {
            var label = root.Find("Column" + ColumnLetter(column)) as RectTransform;
            if (label == null)
            {
                continue;
            }

            var x = (column - columnCenter) * cellSize;
            label.anchoredPosition = new Vector2(x / LabelCanvasScale, bottomY / LabelCanvasScale);
        }
    }

    private Transform CreateLabelRoot()
    {
        var rootObject = new GameObject(LabelRootName, typeof(RectTransform));
        rootObject.hideFlags = HideFlags.DontSave;
        rootObject.transform.SetParent(transform, false);
        var rect = (RectTransform)rootObject.transform;
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = new Vector3(LabelCanvasScale, LabelCanvasScale, LabelCanvasScale);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(100f, 100f);

        var canvas = rootObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 30;
        return rect;
    }

    private void CreateLabel(Transform root, string name, string content, Font font)
    {
        var labelObject = new GameObject(name, typeof(RectTransform));
        labelObject.hideFlags = HideFlags.DontSave;
        labelObject.transform.SetParent(root, false);
        var rect = (RectTransform)labelObject.transform;
        rect.sizeDelta = new Vector2(cellSize / LabelCanvasScale, cellSize / LabelCanvasScale);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        var text = labelObject.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = 48;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.95f, 0.96f, 0.98f, 1f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
    }

    private Font LabelFont()
    {
        if (labelFont != null)
        {
            return labelFont;
        }

        labelFont = ChineseFonts.Legacy;
        if (labelFont == null)
        {
            labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return labelFont;
    }

    private static string ColumnLetter(int column)
    {
        if (column < 0 || column > 25)
        {
            return (column + 1).ToString();
        }

        return ((char)('A' + column)).ToString();
    }

    public bool TryGetFrameCorners(out Vector3 nearLeft, out Vector3 nearRight, out Vector3 farLeft, out Vector3 farRight)
    {
        if (frame == null)
        {
            var child = transform.Find(frameName);
            if (child != null)
            {
                frame = child;
            }
        }

        if (frame == null)
        {
            nearLeft = nearRight = farLeft = farRight = Vector3.zero;
            return false;
        }

        var renderer = frame.GetComponent<SpriteRenderer>();
        var extents = renderer != null && renderer.sprite != null
            ? renderer.sprite.bounds.extents
            : new Vector3(0.5f, 0.5f, 0f);
        nearLeft = frame.TransformPoint(new Vector3(-extents.x, -extents.y, 0f));
        nearRight = frame.TransformPoint(new Vector3(extents.x, -extents.y, 0f));
        farLeft = frame.TransformPoint(new Vector3(-extents.x, extents.y, 0f));
        farRight = frame.TransformPoint(new Vector3(extents.x, extents.y, 0f));
        return true;
    }

    private static void SetLocal(Transform target, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if ((target.localPosition - position).sqrMagnitude > 0.0000001f)
        {
            target.localPosition = position;
        }

        if (Quaternion.Angle(target.localRotation, rotation) > 0.01f)
        {
            target.localRotation = rotation;
        }

        if ((target.localScale - scale).sqrMagnitude > 0.0000001f)
        {
            target.localScale = scale;
        }
    }
}
