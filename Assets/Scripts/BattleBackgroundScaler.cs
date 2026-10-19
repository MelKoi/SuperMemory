using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BattleBackgroundScaler : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool preserveAspectRatio;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        FitToCamera();
    }

    private void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        FitToCamera();
    }

    private void LateUpdate()
    {
        FitToCamera();
    }

    private void FitToCamera()
    {
        Camera cameraToFit = targetCamera != null ? targetCamera : Camera.main;

        if (cameraToFit == null || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float worldHeight;
        float worldWidth;

        if (cameraToFit.orthographic)
        {
            worldHeight = cameraToFit.orthographicSize * 2f;
            worldWidth = worldHeight * cameraToFit.aspect;
        }
        else
        {
            float distance = Mathf.Abs(transform.position.z - cameraToFit.transform.position.z);
            worldHeight = 2f * distance * Mathf.Tan(cameraToFit.fieldOfView * 0.5f * Mathf.Deg2Rad);
            worldWidth = worldHeight * cameraToFit.aspect;
        }

        float scaleX = worldWidth / spriteSize.x;
        float scaleY = worldHeight / spriteSize.y;

        if (preserveAspectRatio)
        {
            float coverScale = Mathf.Max(scaleX, scaleY);
            scaleX = coverScale;
            scaleY = coverScale;
        }

        transform.localScale = new Vector3(scaleX, scaleY, transform.localScale.z);
    }
}
