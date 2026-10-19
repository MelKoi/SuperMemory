using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public sealed class CardDragObject : MonoBehaviour
{
    private Camera dragCamera;
    private Collider2D cardCollider;
    private Vector3 dragOffset;
    private bool isDragging;

    private void Reset()
    {
        transform.position = Vector3.zero;
    }

    private void Awake()
    {
        cardCollider = GetComponent<Collider2D>();

        if (Camera.main != null)
        {
            dragCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!TryGetPointerState(out Vector2 pointerPosition, out bool pressed, out bool pressedThisFrame))
        {
            isDragging = false;
            return;
        }

        if (pressedThisFrame && IsPointerOverCard(pointerPosition))
        {
            BeginDrag(pointerPosition);
        }

        if (!isDragging)
        {
            return;
        }

        if (!pressed)
        {
            isDragging = false;
            return;
        }

        Drag(pointerPosition);
    }

    private void BeginDrag(Vector2 pointerPosition)
    {
        dragCamera = Camera.main != null ? Camera.main : Camera.current;

        if (dragCamera == null)
        {
            return;
        }

        if (TryGetPointerWorldPoint(pointerPosition, out Vector3 mouseWorldPoint))
        {
            dragOffset = transform.position - mouseWorldPoint;
            isDragging = true;
        }
    }

    private void Drag(Vector2 pointerPosition)
    {
        if (dragCamera == null)
        {
            return;
        }

        if (TryGetPointerWorldPoint(pointerPosition, out Vector3 mouseWorldPoint))
        {
            transform.position = mouseWorldPoint + dragOffset;
        }
    }

    private bool IsPointerOverCard(Vector2 pointerPosition)
    {
        dragCamera = Camera.main != null ? Camera.main : Camera.current;

        if (dragCamera == null)
        {
            return false;
        }

        return TryGetPointerWorldPoint(pointerPosition, out Vector3 worldPoint)
            && cardCollider.OverlapPoint(worldPoint);
    }

    private bool TryGetPointerWorldPoint(Vector2 pointerPosition, out Vector3 worldPoint)
    {
        if (dragCamera == null)
        {
            worldPoint = default;
            return false;
        }

        float distanceToObject = transform.position.z - dragCamera.transform.position.z;
        Vector3 screenPoint = new Vector3(pointerPosition.x, pointerPosition.y, distanceToObject);
        worldPoint = dragCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = transform.position.z;

        return true;
    }

    private static bool TryGetPointerState(out Vector2 pointerPosition, out bool pressed, out bool pressedThisFrame)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            pointerPosition = default;
            pressed = false;
            pressedThisFrame = false;
            return false;
        }

        pointerPosition = mouse.position.ReadValue();
        pressed = mouse.leftButton.isPressed;
        pressedThisFrame = mouse.leftButton.wasPressedThisFrame;
        return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
        pointerPosition = Input.mousePosition;
        pressed = Input.GetMouseButton(0);
        pressedThisFrame = Input.GetMouseButtonDown(0);
        return true;
#else
        pointerPosition = default;
        pressed = false;
        pressedThisFrame = false;
        return false;
#endif
    }
}
