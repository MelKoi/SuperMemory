using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int direction;
    public float speed;
    public float maxDistance;
    public LayerMask collisionLayer;
    public Vector3 startPosition;
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

