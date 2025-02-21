using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class CardDragContral : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 在CardDragContral.cs顶部添加引用
    [Header("武器蓄能部分")]
    private Transform Weapon1;
    private Transform Weapon2;
    [SerializeField]
    private PlayerAsset CardUser;


    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;

    [Header("References")]
    [SerializeField] private Transform handArea; // 拖入手牌区父物体

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        originalParent = transform.parent;
        handArea = transform.parent;

        //获取武器
        Weapon1 = transform.parent.parent.Find("WeaponCard1");
        Weapon2 = transform.parent.parent.Find("WeaponCard2");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 提升层级至Canvas顶层
        transform.SetParent(handArea.parent); // 假设handArea在Canvas下
        transform.SetAsLastSibling();

        // 记录初始状态
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 跟随鼠标移动
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 判断是否拖出手牌区
        if(RectTransformUtility.RectangleContainsScreenPoint(
            Weapon1.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            Debug.Log("蓄能！");
            CardUser.Weapon1Acc++;
            Destroy(gameObject);
            //触发武器蓄能

        }
        else if(RectTransformUtility.RectangleContainsScreenPoint(
            Weapon2.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            Debug.Log("蓄能！");
            CardUser.Weapon2Acc++;
            Destroy(gameObject);
            //触发武器蓄能
        }
        else if (!RectTransformUtility.RectangleContainsScreenPoint(
            handArea.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            // 触发卡牌使用逻辑
            Debug.Log("卡牌已拖出手牌区！");
            HandleCardUsage();
        }
        else
        {
            // 返回原位动画
            rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => transform.SetParent(originalParent));
        }
    }

    private void HandleCardUsage()
    {
        // 在此处实现卡牌效果逻辑
        OneCardManager cardManager = GetComponent<OneCardManager>();
        if(cardManager == null)
        {
            Debug.LogError("卡牌缺少OneCardManager组件");
            return;
        }
        Destroy(gameObject);
    }
}
