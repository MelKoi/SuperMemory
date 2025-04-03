using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardPreview : MonoBehaviour
{
    public void OnPointerEnter()
    {
         mRect.DOKill();  
         mRect.DOScale(new Vector3(1.0f, 1.0f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit()
    {
        mRect.DOKill();
        mRect.DOScale(new Vector3(0.75f, 0.75f), 0.5f).SetEase(Ease.OutExpo);
    }
    private RectTransform mRect;
    // Start is called before the first frame update
    void Start()
    {
        mRect=GetComponent<RectTransform>(); 
    }

    // Update is called once per frame
    void Update()
    {
        transform.DORotate(new Vector3(0, 0, 0), 0.8f);
    }
}
