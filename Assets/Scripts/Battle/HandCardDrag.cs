using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 手牌拖动。松手在手牌区里则放回。拖到自己的武器上是蓄能，拖出手牌区是打出。
// 打出或蓄能被接受后，这张牌已经离开手牌，拖动中的显示立刻去掉，不留在画面上。
public sealed class CardDropTarget : MonoBehaviour
{
    public bool Charge;
    public int WeaponSlot;
}

public sealed class HandCardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    RectTransform rect;
    Canvas canvas;
    CanvasGroup group;
    Transform home;
    int sibling;
    Vector2 homePosition;
    CardInstance card;
    HandZone zone;
    Func<CardInstance, CardDropTarget, bool> dropped;

    public void Bind(CardInstance instance, HandZone owner, Func<CardInstance, CardDropTarget, bool> onDrop)
    {
        card = instance;
        zone = owner;
        dropped = onDrop;
        rect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rect == null || canvas == null)
        {
            return;
        }

        home = rect.parent;
        sibling = rect.GetSiblingIndex();
        homePosition = rect.anchoredPosition;
        group.blocksRaycasts = false;
        if (zone != null)
        {
            zone.NotifyDrag(true);
        }
        rect.SetParent(canvas.transform, true);
        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rect == null)
        {
            return;
        }

        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (group != null)
        {
            group.blocksRaycasts = true;
        }

        if (zone != null)
        {
            zone.NotifyDrag(false);
        }

        var accepted = !InsideHand(eventData) && dropped != null && dropped(card, FindTarget(eventData));
        if (accepted)
        {
            Destroy(gameObject);
            return;
        }

        if (home == null || rect == null)
        {
            return;
        }

        rect.SetParent(home, false);
        rect.SetSiblingIndex(sibling);
        rect.anchoredPosition = homePosition;
    }

    bool InsideHand(PointerEventData eventData)
    {
        return zone != null && zone.ScreenRect.Contains(eventData.position);
    }

    static CardDropTarget FindTarget(PointerEventData eventData)
    {
        if (EventSystem.current == null)
        {
            return null;
        }

        var hits = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, hits);
        for (var i = 0; i < hits.Count; i++)
        {
            var target = hits[i].gameObject.GetComponentInParent<CardDropTarget>();
            if (target != null)
            {
                return target;
            }
        }

        return null;
    }
}
