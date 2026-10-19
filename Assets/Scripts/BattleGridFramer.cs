using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(50)]
[RequireComponent(typeof(Camera))]
public sealed class BattleGridFramer : MonoBehaviour
{
    [SerializeField] private BattleBoardPerspective board;
    [SerializeField] private float fieldOfView = 40f;
    [SerializeField] private float cameraHeight = 5.5f;
    [SerializeField] private float lookAlong = 0.4f;
    [SerializeField] private float marginX = 0.14f;
    [SerializeField] private float marginY = 0.06f;

    private Camera boardCamera;
    private readonly Vector3[] corners = new Vector3[4];

    private void OnEnable()
    {
        boardCamera = GetComponent<Camera>();
        FrameBoard();
    }

    private void OnValidate()
    {
        boardCamera = GetComponent<Camera>();
        FrameBoard();
    }

    private void LateUpdate()
    {
        FrameBoard();
    }

    public void FrameBoard()
    {
        if (boardCamera == null)
        {
            boardCamera = GetComponent<Camera>();
        }

        if (board == null)
        {
            board = FindFirstObjectByType<BattleBoardPerspective>();
        }

        if (boardCamera == null || board == null)
        {
            return;
        }

        if (boardCamera.aspect <= 0.01f)
        {
            boardCamera.aspect = 16f / 9f;
        }

        board.Apply();
        if (!board.TryGetFrameCorners(out corners[0], out corners[1], out corners[2], out corners[3]))
        {
            return;
        }

        var nearCenter = (corners[0] + corners[1]) * 0.5f;
        var farCenter = (corners[2] + corners[3]) * 0.5f;
        var focus = Vector3.Lerp(nearCenter, farCenter, lookAlong);

        boardCamera.orthographic = false;
        boardCamera.fieldOfView = fieldOfView;

        var near = 9f;
        var far = 34f;
        var best = far;
        for (var i = 0; i < 16; i++)
        {
            var distance = (near + far) * 0.5f;
            Place(focus, distance);
            if (BoardFits())
            {
                best = distance;
                far = distance;
            }
            else
            {
                near = distance;
            }
        }

        Place(focus, best);
    }

    private void Place(Vector3 focus, float distance)
    {
        var position = new Vector3(0f, cameraHeight, -distance);
        var rotation = Quaternion.LookRotation(focus - position, Vector3.up);
        if ((boardCamera.transform.position - position).sqrMagnitude > 0.000001f)
        {
            boardCamera.transform.position = position;
        }

        if (Quaternion.Angle(boardCamera.transform.rotation, rotation) > 0.01f)
        {
            boardCamera.transform.rotation = rotation;
        }
    }

    private bool BoardFits()
    {
        for (var i = 0; i < corners.Length; i++)
        {
            var viewport = boardCamera.WorldToViewportPoint(corners[i]);
            if (viewport.z <= 0.01f)
            {
                return false;
            }

            if (viewport.x < marginX || viewport.x > 1f - marginX || viewport.y < marginY || viewport.y > 1f - marginY)
            {
                return false;
            }
        }

        return true;
    }
}
