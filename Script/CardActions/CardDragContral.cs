using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

[RequireComponent(typeof(RectTransform))]
public class CardDragContral : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 在CardDragContral.cs顶部添加引用
    public bool canDrug = true;
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
        if(transform.parent.name.Equals("EnemyHand"))
        {
            canDrug = false;
            //gameObject.GetComponent<OneCardManager>().CardBack.SetActive(true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(canDrug)
        {
            // 记录初始状态
            originalPosition = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            // 提升层级至Canvas顶层
            transform.SetParent(handArea.parent); // 假设handArea在Canvas下
            transform.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(canDrug)
        // 跟随鼠标移动
            rectTransform.anchoredPosition += eventData.delta / transform.root.localScale.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrug)
            return;
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
        else if (!RectTransformUtility.RectangleContainsScreenPoint(//发现了无法移动原处的bug
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
            // 直接瞬移到初始位置
            rectTransform.anchoredPosition = originalPosition;
            transform.SetParent(originalParent); // 恢复父物体
            Debug.Log("卡牌已返回原位！");
        }
    }

    private void HandleCardUsage()
    {
        int UserSp = CardUser.NowSp;//记录玩家现在的sp
        // 在此处实现卡牌效果逻辑
        OneCardManager cardManager = GetComponent<OneCardManager>();
        if(cardManager == null)
        {
            Debug.LogError("卡牌缺少OneCardManager组件");
            return;
        }
        BattleManager battleManager = transform.parent.parent.GetComponent<BattleManager>();
        if (battleManager._currentPhase == GamePhase.playerAction)
            battleManager.UseCard(cardManager.cardAsset,gameObject,battleManager.Player);
        else if(battleManager._currentPhase == GamePhase.enemyAction)
            battleManager.UseCard(cardManager.cardAsset, gameObject, battleManager.Enemy);
        if(UserSp != CardUser.NowSp)
            rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => transform.SetParent(originalParent));
    }
}
