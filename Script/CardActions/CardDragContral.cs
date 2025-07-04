﻿using System.Collections;
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

    public CardAnimationController cardAnimationController;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;

    public float rotationSpeed = 5f; // 旋转速度
    public float tweenDuration = 0.5f; // Tween动画持续时间
    public float maxRotationAngle = 90f; // 最大旋转角度（正负45°）

    private Vector3 _lastMousePosition;


    [Header("References")]
    [SerializeField] private Transform handArea; // 拖入手牌区父物体
    [SerializeField] private BattleManager PlayerBattle;//战斗管理器脚本

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cardAnimationController = GetComponent<CardAnimationController>();
        originalParent = transform.parent;
        handArea = transform.parent;
        PlayerBattle = handArea.parent.parent.GetComponent<BattleManager>();

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
        if (canDrug)
        {
            // 跟随鼠标移动
            rectTransform.anchoredPosition += eventData.delta / transform.root.localScale.x;
            //旋转
            // 计算鼠标移动方向
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - _lastMousePosition;

            // 计算目标旋转角度
            float targetRotationZ = -mouseDelta.x * rotationSpeed;

            // 限制旋转角度在正负45°内
            targetRotationZ = Mathf.Clamp(targetRotationZ, -maxRotationAngle, maxRotationAngle);

            // 使用DOTween平滑旋转卡牌
            transform.DORotate(new Vector3(0, 0, targetRotationZ), tweenDuration);

            // 更新上一帧鼠标位置
            _lastMousePosition = currentMousePosition;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrug)
            return;
        StartCoroutine(OnEndDragContinue(eventData));
       
    }

    private IEnumerator OnEndDragContinue(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 判断是否拖出手牌区
        if (RectTransformUtility.RectangleContainsScreenPoint(
            Weapon1.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            Debug.Log("蓄能！");
            CardUser.Weapon1Acc++;
            Destroy(gameObject);
            PlayerBattle.BS.accEvent.RaiseEvent();
            //触发武器蓄能
            yield break; // 结束协程
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(
            Weapon2.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            Debug.Log("蓄能！");
            CardUser.Weapon2Acc++;
            Destroy(gameObject);
            PlayerBattle.BS.accEvent.RaiseEvent();
            //触发武器蓄能
            yield break; // 结束协程
        }
        else if (!RectTransformUtility.RectangleContainsScreenPoint(//发现了无法移动原处的bug
            handArea.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera)
            && handArea.transform.parent.parent.GetComponent<BattleManager>().Player.NowMp > 
            int.Parse(GetComponent<OneCardManager>().cardAsset.cost))
        {
            // 触发卡牌使用逻辑
            Debug.Log("卡牌已拖出手牌区！");
            yield return StartCoroutine(cardAnimationController.PlayAnimation());
            HandleCardUsage();
        }
        else
        {
            // 直接瞬移到初始位置
            rectTransform.anchoredPosition = originalPosition;
            transform.DORotate(new Vector3(0, 0, 0), tweenDuration);
            transform.SetParent(originalParent); // 恢复父物体
            Debug.Log("卡牌已返回原位！");
        }
    }

    private void HandleCardUsage()
    {
        int UserSp = CardUser.NowMp;//记录玩家现在的sp
        // 在此处实现卡牌效果逻辑
        OneCardManager cardManager = GetComponent<OneCardManager>();
        if(cardManager == null)
        {
            Debug.LogError("卡牌缺少OneCardManager组件");
            return;
        }
        BattleManager battleManager = transform.parent.parent.GetComponent<BattleManager>();
        if (battleManager._currentPhase == GamePhase.playerAction)
        {
            battleManager.UseCard(cardManager.cardAsset, gameObject, battleManager.Player);
        }
        if(UserSp != CardUser.NowMp)
            rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => transform.SetParent(originalParent));
    }
}
