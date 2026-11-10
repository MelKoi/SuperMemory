using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 局内手牌按影之诗的位置和查看方式摆。
// 自己的牌贴在屏幕底边，互相重叠，只露出上沿。指针放上来，整手牌上移并摊开；离开后收回。
// 对方的牌背贴在屏幕顶边，不展开。
public sealed class HandZone : MonoBehaviour
{
    const float CardWidth = 150f;
    const float CardHeight = 214f;
    const float Follow = 14f;

    [SerializeField] private BattleSide side;

    readonly List<RectTransform> cards = new List<RectTransform>();
    Canvas canvas;
    bool faceDown;
    bool opened;
    bool dragging;
    bool snap;
    int frontIndex = -1;

    public BattleSide Side => side;
    public RectTransform Area => (RectTransform)transform;
    public bool Opened => opened;

    public void Initialize(BattleSide owner)
    {
        side = owner;
        canvas = GetComponentInParent<Canvas>();
    }

    public void NotifyDrag(bool active)
    {
        dragging = active;
        if (active)
        {
            opened = true;
        }
    }

    public void Clear()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        cards.Clear();
        opened = false;
        dragging = false;
        frontIndex = -1;
    }

    public void Present(CardCatalog catalog, IReadOnlyList<CardInstance> hand, Func<CardInstance, CardDropTarget, bool> onDrop)
    {
        Clear();
        faceDown = false;
        snap = true;
        if (catalog == null || hand == null)
        {
            return;
        }

        for (var i = 0; i < hand.Count; i++)
        {
            var instance = hand[i];
            var definition = catalog.Get(instance.Kind, instance.CardId);
            var view = CreateCard(definition.Kind);
            if (view == null)
            {
                continue;
            }

            view.name = definition.Name;
            view.ApplyKind(definition.Kind);
            view.Present(definition.Name, CardView.EffectLine(definition), definition.Cost, definition.Subtype);
            view.SetFaceUp(true);
            var drag = view.gameObject.AddComponent<HandCardDrag>();
            drag.Bind(instance, this, onDrop);
        }
    }

    public void PresentBacks(int count)
    {
        Clear();
        faceDown = true;
        opened = false;
        snap = true;
        for (var i = 0; i < count; i++)
        {
            var view = CreateCard(CardKind.Action);
            if (view == null)
            {
                continue;
            }

            view.name = "牌背";
            view.SetFaceUp(false);
            var graphics = view.GetComponentsInChildren<Graphic>(true);
            for (var g = 0; g < graphics.Length; g++)
            {
                graphics[g].raycastTarget = false;
            }
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

    CardView CreateCard(CardKind kind)
    {
        var prefab = CardPrefabLibrary.For(kind);
        if (prefab == null)
        {
            return null;
        }

        var view = Instantiate(prefab, transform);
        var rect = (RectTransform)view.transform;
        var edge = faceDown ? 1f : 0f;
        rect.anchorMin = new Vector2(0.5f, edge);
        rect.anchorMax = new Vector2(0.5f, edge);
        rect.pivot = new Vector2(0.5f, edge);
        rect.sizeDelta = new Vector2(CardWidth, CardHeight);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        cards.Add(rect);
        return view;
    }

    void Update()
    {
        if (!Application.isPlaying || faceDown || side != BattleSide.Player)
        {
            opened = false;
            frontIndex = -1;
            return;
        }

        if (!TryPointer(out var pointer))
        {
            return;
        }

        if (dragging)
        {
            opened = true;
        }
        else if (opened)
        {
            opened = Inside(pointer, true) || Inside(pointer, false);
        }
        else
        {
            opened = Inside(pointer, false);
        }

        frontIndex = opened ? FrontCard(pointer) : -1;
    }

    void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Layout();
        snap = false;
    }

    void Layout()
    {
        var count = cards.Count;
        if (count == 0)
        {
            return;
        }

        var blend = snap ? 1f : 1f - Mathf.Exp(-Follow * Time.deltaTime);
        for (var i = 0; i < count; i++)
        {
            var rect = cards[i];
            if (rect == null || rect.parent != transform)
            {
                continue;
            }

            var lifted = i == frontIndex;
            Target(i, opened, lifted, out var position, out var scale);
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, position, blend);
            rect.localScale = Vector3.Lerp(rect.localScale, new Vector3(scale, scale, 1f), blend);
            rect.localRotation = Quaternion.identity;
            rect.SetSiblingIndex(i);
        }

        if (frontIndex >= 0 && frontIndex < count && cards[frontIndex] != null && cards[frontIndex].parent == transform)
        {
            cards[frontIndex].SetAsLastSibling();
        }
    }

    void Target(int index, bool expanded, bool lifted, out Vector2 position, out float scale)
    {
        var count = cards.Count;
        if (faceDown)
        {
            scale = 0.36f;
            var step = FitStep(count, 22f, 520f, CardWidth * scale);
            position = new Vector2(Centered(index, count, step), -6f);
            return;
        }

        scale = expanded ? 1.02f : 0.66f;
        var preferred = expanded ? 158f : 48f;
        var width = expanded ? 1180f : 860f;
        var y = expanded ? 12f : -78f;
        if (lifted)
        {
            y += 18f;
        }

        var used = FitStep(count, preferred, width, CardWidth * scale);
        position = new Vector2(Centered(index, count, used), y);
    }

    static float Centered(int index, int count, float step)
    {
        return -((count - 1) * step) * 0.5f + index * step;
    }

    static float FitStep(int count, float preferred, float maxWidth, float cardWidth)
    {
        if (count <= 1)
        {
            return 0f;
        }

        var room = Mathf.Max(0f, maxWidth - cardWidth);
        return Mathf.Min(preferred, room / (count - 1));
    }

    bool Inside(Vector2 screen, bool expanded)
    {
        var found = false;
        var union = new Rect();
        for (var i = 0; i < cards.Count; i++)
        {
            Target(i, expanded, false, out var position, out var scale);
            var rect = ScreenBounds(position, scale);
            union = found ? Union(union, rect) : rect;
            found = true;
        }

        if (!found)
        {
            return false;
        }

        union.xMin -= 12f;
        union.xMax += 12f;
        union.yMax += 24f;
        return union.Contains(screen);
    }

    int FrontCard(Vector2 screen)
    {
        for (var i = cards.Count - 1; i >= 0; i--)
        {
            Target(i, true, false, out var position, out var scale);
            if (ScreenBounds(position, scale).Contains(screen))
            {
                return i;
            }
        }

        return -1;
    }

    Rect ScreenBounds(Vector2 position, float scale)
    {
        var half = CardWidth * scale * 0.5f;
        var height = CardHeight * scale;
        var bottom = faceDown ? position.y - height : position.y;
        var min = ScreenOf(new Vector2(position.x - half, bottom));
        var max = ScreenOf(new Vector2(position.x + half, bottom + height));
        return Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
    }

    Vector2 ScreenOf(Vector2 local)
    {
        var world = Area.TransformPoint(new Vector3(local.x, local.y, 0f));
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.WorldToScreenPoint(camera, world);
    }

    static Rect Union(Rect a, Rect b)
    {
        return Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));
    }

    static bool TryPointer(out Vector2 position)
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse == null)
        {
            position = default;
            return false;
        }

        position = mouse.position.ReadValue();
        return true;
#else
        position = Input.mousePosition;
        return true;
#endif
    }

    void OnDrawGizmosSelected()
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
