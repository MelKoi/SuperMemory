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

    private float dir;
    private Vector3 currentPosition;
    private Vector3 currentScale ;
    private bool isHide;
    private bool isAcc;
    private bool isAtt;

    public GameObject Bullet;//子弹预制体
    public float bulletSpeed = 10f;
    public float maxBulletDistance = 10f; // 子弹最大飞行距离
    public LayerMask collisionLayer; // 设置要检测的碰撞层
    public BulletController bulletController = null;//子弹管理


    private void Awake()
    {
        dir = isPlayer ? -1 : 1;
        currentPosition = transform.position;
        currentScale = transform.localScale;
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
            bulletController = bullet.AddComponent<BulletController>();
            // 设置子弹参数
            sequence.Append(transform.DOScaleX(transform.localScale.x + 1, duration));
            sequence.AppendInterval(wait);
           
            int direction = isPlayer ? 1 : -1; // 玩家向右，敌人向左
            sequence.Append(transform.DOScaleX(currentScale.x, duration)
                            .SetEase(Ease.OutQuad));
            sequence.OnComplete(() => {
                bulletController.Initialize(direction, bulletSpeed, maxBulletDistance, collisionLayer);
                isAtt = false;
            });           
            sequence.Play();
        }
    }
}
public class BulletController : MonoBehaviour
{
    private int direction;
    private float speed;
    private float maxDistance;
    private LayerMask collisionLayer;
    private Vector3 startPosition;
    public bool hasHit;

    public void Initialize(int dir, float spd, float maxDist, LayerMask layer)
    {
        direction = dir;
        speed = spd;
        maxDistance = maxDist;
        collisionLayer = layer;
        startPosition = transform.position;

        // 设置子弹朝向
        if (direction < 0)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y * 1, transform.localScale.z * 1);
        }

        // 开始移动
        StartCoroutine(MoveBullet());
    }

    private IEnumerator MoveBullet()
    {
        while (Vector3.Distance(startPosition, transform.position) < maxDistance && !hasHit)
        {
            // 检测碰撞
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.right * direction,
                speed * Time.deltaTime,
                collisionLayer);

            if (hit.collider != null)
            {
                // 命中目标
                HandleHit(hit.collider);
                yield break;
            }

            // 移动子弹
            transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
            yield return null;
        }

        // 到达最大距离未命中
        Destroy(gameObject);
    }

    private void HandleHit(Collider2D other)
    {
        hasHit = true;
        Debug.Log("子弹命中: " + other.gameObject.name);

         //这里可以添加命中效果，如爆炸动画等

        Destroy(gameObject);
    }

    // 可选：使用物理碰撞检测替代射线检测
    //private void OnTriggerEnter2D(Collider2D other)
    //{
        //if (((1 << other.gameObject.layer) & collisionLayer) != 0)
        //{
            //HandleHit(other);
        //}
    //}
}
