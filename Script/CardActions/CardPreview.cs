using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardPreview : MonoBehaviour
{
    public bool choosed = false;
    private BattleManager battleManager;
    private GameObject gj;
    public void OnPointerEnter()
    {
         mRect.DOKill();  
         mRect.DOScale(new Vector3(1.0f, 1.0f), 0.5f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit()
    {
        mRect.DOKill();
        if (!choosed)
        {
            mRect.DOScale(new Vector3(0.75f, 0.75f), 0.5f).SetEase(Ease.OutExpo);
        } 
    }
    public void OnPointerDown()
    {
        if (!choosed)
        {
            choosed = true;
            gj = this.gameObject;
            battleManager.ChooseCards.Add(gj);
            mRect.DOKill();
            mRect.DOScale(new Vector3(1.0f, 1.0f), 0.5f).SetEase(Ease.OutExpo);
        }
        else if (choosed)
        {
            choosed = false;
            gj = this.gameObject;
            battleManager.ChooseCards.Remove(gj);
            mRect.DOKill();
            mRect.DOScale(new Vector3(0.75f, 0.75f), 0.5f).SetEase(Ease.OutExpo);
        }
    }
    private RectTransform mRect;
    // Start is called before the first frame update
    void Start()
    {
        mRect=GetComponent<RectTransform>();
        battleManager = GameObject.Find("BattlePlace").GetComponent<BattleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
