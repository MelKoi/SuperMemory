using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverPreview : MonoBehaviour//卡牌预览
{
    public GameObject TurnThisOffWhenPreviewing;//展示是将原件的Active隐藏
    public Vector3 TargetPosition;//展示时的坐标
    public float TargetScale;//展示时的大小
    public GameObject previewGameObject;//展示的组件（一般是原件的备份
    public bool ActivateInAwake = false;//？

    private static HoverPreview currentlyViewing = null;//储存当前预览的内容和项目
    
    private static bool _PreviewsAllowed = true;

    public static bool PreviewsAllowed//是否允许全局预览（在整个项目中都能进行控制
    {
        get { return _PreviewsAllowed; }

        set
        {
            _PreviewsAllowed=value;//从外部设定
            if (!_PreviewsAllowed)//拖拽的时候不允许预览
                StopAllPreviews();
        }
    }

    private bool _thisPreviewEnabled = false;//允许每个部分的预览
    public bool ThisPreviewEnabled
    {
        get { return _thisPreviewEnabled; }
        set
        {
            _thisPreviewEnabled=value;
            if (!_thisPreviewEnabled)
                StopThisPreview();
        }
    }

    public bool OverCollider { get; set; }

    void Awake()
    {
        ThisPreviewEnabled = ActivateInAwake;
    }
    private void OnMouseEnter()
    {
        OverCollider = true;
        if (PreviewsAllowed && ThisPreviewEnabled)
            PreviewThisObject();
    }
    private void OnMouseExit()
    {
        OverCollider = false;

        if (!PreviewingSomeCard())
            StopAllPreviews();
    }
    void PreviewThisObject()//预览当前物品
    {
        StopAllPreviews();

        currentlyViewing=this;//储存目前需要预览的物品

        previewGameObject.SetActive(true);
        
        if(TurnThisOffWhenPreviewing!=null)
            TurnThisOffWhenPreviewing.SetActive(false);

        previewGameObject.transform.localPosition = Vector3.zero;
        previewGameObject.transform.localScale = Vector3.one;

        previewGameObject.transform.DOLocalMove(TargetPosition, 1f);
        previewGameObject.transform.DOScale(TargetScale, 1f);
    }
    void StopThisPreview()//停下特定卡牌的预览
    {
        previewGameObject.SetActive(false);
        previewGameObject.transform.localPosition= Vector3.zero;
        previewGameObject.transform.localScale= Vector3.one;//回归物品的位置和大小
        if (TurnThisOffWhenPreviewing != null)
            TurnThisOffWhenPreviewing.SetActive(true);
    }

    private static void StopAllPreviews()
    {
        if(currentlyViewing != null)
        {
            currentlyViewing.StopThisPreview();
        }
    }

    private static bool PreviewingSomeCard()
    {
        if(!PreviewsAllowed)
            return false;

        HoverPreview[] allHoverBlowups = GameObject.FindObjectsOfType<HoverPreview>();//用来获取所有这个属性的物体

        foreach(HoverPreview hb in allHoverBlowups)
        {
            if (hb.OverCollider && hb.ThisPreviewEnabled)
                return true;
        }

        return false;
    }
}
