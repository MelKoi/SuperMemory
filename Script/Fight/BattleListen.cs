using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public class BattleListen : MonoBehaviour
{
    [Header("事件监听")]
    public VoidEventSO hideEvent;
    public VoidEventSO accEvent;
    public VoidEventSO attEvent;

    [Header("基本参数")]
    public bool isPlayer;
    public float hideDistance;
    public float duration;
    public float wait;
    public Transform player;
    public Transform enemy;

    private float dir;
    private Vector3 currentPosition;
    private Vector3 currentScale ;
    private Vector3 playerTrans;
    private Vector3 enemyTrans;
    private bool isHide;
    private bool isAcc;
    private bool isAtt;

    public GameObject Bullet;//子弹预制体
    public float bulletSpeed = 10f;
    public float maxBulletDistance; // 子弹最大飞行距离
    public LayerMask collisionLayer; // 设置要检测的碰撞层
    public BulletController bulletController = null;//子弹管理


    private void Awake()
    {
        dir = isPlayer ? -1 : 1;
        currentPosition = transform.position;
        currentScale = transform.localScale;
        playerTrans = player.localPosition;
        enemyTrans = enemy.localPosition;
        maxBulletDistance = Mathf.Abs(playerTrans.x - enemyTrans.x);
    }
    private void OnEnable()         //设置监听
    {
        hideEvent.OnEventRiased += OnHideEvent;
        accEvent.OnEventRiased += OnAccEvent;
        attEvent.OnEventRiased += OnAttEvent;
    }
    private void OnDisable()        //移除监听
    {
        hideEvent.OnEventRiased -= OnHideEvent;
        accEvent.OnEventRiased -= OnAccEvent;
        attEvent.OnEventRiased -= OnAttEvent;
    }

    //闪避监听
    public void OnHideEvent()
    {
        if (!isHide) {
            Debug.Log("闪避");
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
       
        if (!isAcc)
        {   
            Debug.Log("蓄力");
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScaleY(transform.localScale.y + 1, duration));
            sequence.AppendInterval(wait);
            sequence.Append(transform.DOScaleY(currentScale.y, duration)
                            .SetEase(Ease.OutQuad));
            sequence.OnComplete(() => isAcc = false);
            sequence.Play();
        }
    }

    //攻击监听
    public void OnAttEvent()
    {
        if (!isAtt)
        {
            Debug.Log("攻击");
            Sequence sequence = DOTween.Sequence();
            GameObject bullet = Instantiate(Bullet, transform.position, Quaternion.identity);
            int direction = isPlayer ? 1 : -1; // 玩家向右，敌人向左
            bulletController = bullet.GetComponent<BulletController>();
            bulletController.Initialize(direction, bulletSpeed, maxBulletDistance, collisionLayer);
            // 设置子弹参数
            sequence.Append(transform.DOScaleX(transform.localScale.x + 1, duration));
            sequence.AppendInterval(wait);
            sequence.Append(transform.DOScaleX(currentScale.x, duration)
                            .SetEase(Ease.OutQuad));
            sequence.OnComplete(() => {
                isAtt = false;
                
            });           
            sequence.Play();
        }
    }
}
