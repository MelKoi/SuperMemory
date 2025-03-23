using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAnimationController : MonoBehaviour
{
    [Header("动画组件")]
    public Animator anim;
    public bool isUsing;
    public bool useFinish;
    public bool isGreenCard;

    [HideInInspector] public OneCardManager manager;

    private void Start()
    {
        anim = GetComponent<Animator>();
        manager = GetComponent<OneCardManager>();
        if (manager.cardAsset.Type == Type.战技)
            isGreenCard = true;
        anim.SetBool("isGreenCard", isGreenCard);
    }

    public void bofa()
    {
        StartCoroutine(PlayAnimation());
    }

    //等待动画播放完毕
    public IEnumerator PlayAnimation()
    {
        isUsing = true;
        anim.SetBool("isUsing", isUsing);
        
        yield return new WaitUntil(() => useFinish);
        Debug.Log("播放完毕");
    }
}
