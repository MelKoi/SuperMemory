using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BattleListen : MonoBehaviour
{
    [Header("事件监听")]
    public VoidEventSO hideEvent;
    public VoidEventSO accEvent;

    [Header("基本参数")]
    public bool isPlayer;
    public float hideDistance;
    public float duration;
    public float wait;

    private float dir;
    private Vector3 currentPosition;
    private Color currentColor;
    private bool isHide;
    private bool isAcc;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        dir = isPlayer ? -1 : 1;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()         //设置监听
    {
        hideEvent.OnEventRiased += OnHideEvent;
        accEvent.OnEventRiased += OnAccEvent;
    }
    private void OnDisable()        //移除监听
    {
        hideEvent.OnEventRiased -= OnHideEvent;
        accEvent.OnEventRiased -= OnAccEvent;
    }

    //闪避监听
    public void OnHideEvent()
    {
        if (!isHide) {
            Debug.Log("闪避");
            currentPosition = transform.position;
            isHide = true;
            Sequence sequence = DOTween.Sequence();
            //第一阶段：向指定方向移动
            sequence.Append(transform.DOMoveX(currentPosition.x + dir * hideDistance, duration));
            //第二阶段：停留wait秒
            sequence.AppendInterval(wait);
            //第三阶段：移回原位
            sequence.Append(transform.DOMoveX(currentPosition.x, duration)
                            .SetEase(Ease.OutQuad));
            // 动画结束时重置标志
            sequence.OnComplete(() => isHide = false);
            //开始播放序列
            sequence.Play();
        }
        
    }

    //蓄力监听
    public void OnAccEvent()
    {
        Sequence sequence = DOTween.Sequence();
        isAcc = !isAcc;
        currentColor = spriteRenderer .color;
        if (isAcc)
        {
            Debug.Log("蓄力");
            //这些DOTween终究只是暂时的演示，后期换成动画控制器即可
            sequence.Append(spriteRenderer.DOColor(new Color(0.91f, 0.32f, 0.3f),0.5f)
                                                                  .SetLoops(-1,LoopType.Yoyo)
                                                                  .OnComplete(() =>{
                                                                      spriteRenderer.DOColor(currentColor, 0.5f);
                                                                  }));
            sequence.Play();
        }
    }
}
