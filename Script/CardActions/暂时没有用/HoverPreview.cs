using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverPreview : MonoBehaviour//����Ԥ��
{
    public GameObject TurnThisOffWhenPreviewing;//չʾ�ǽ�ԭ����Active����
    public Vector3 TargetPosition;//չʾʱ������
    public float TargetScale;//չʾʱ�Ĵ�С
    public GameObject previewGameObject;//չʾ�������һ����ԭ���ı���
    public bool ActivateInAwake = false;//��

    private static HoverPreview currentlyViewing = null;//���浱ǰԤ�������ݺ���Ŀ
    
    private static bool _PreviewsAllowed = true;

    public static bool PreviewsAllowed//�Ƿ�����ȫ��Ԥ������������Ŀ�ж��ܽ��п���
    {
        get { return _PreviewsAllowed; }

        set
        {
            _PreviewsAllowed=value;//���ⲿ�趨
            if (!_PreviewsAllowed)//��ק��ʱ������Ԥ��
                StopAllPreviews();
        }
    }

    private bool _thisPreviewEnabled = false;//����ÿ�����ֵ�Ԥ��
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
    void PreviewThisObject()//Ԥ����ǰ��Ʒ
    {
        StopAllPreviews();

        currentlyViewing=this;//����Ŀǰ��ҪԤ������Ʒ

        previewGameObject.SetActive(true);
        
        if(TurnThisOffWhenPreviewing!=null)
            TurnThisOffWhenPreviewing.SetActive(false);

        previewGameObject.transform.localPosition = Vector3.zero;
        previewGameObject.transform.localScale = Vector3.one;

        previewGameObject.transform.DOLocalMove(TargetPosition, 1f);
        previewGameObject.transform.DOScale(TargetScale, 1f);
    }
    void StopThisPreview()//ͣ���ض����Ƶ�Ԥ��
    {
        previewGameObject.SetActive(false);
        previewGameObject.transform.localPosition= Vector3.zero;
        previewGameObject.transform.localScale= Vector3.one;//�ع���Ʒ��λ�úʹ�С
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

        HoverPreview[] allHoverBlowups = GameObject.FindObjectsOfType<HoverPreview>();//������ȡ����������Ե�����

        foreach(HoverPreview hb in allHoverBlowups)
        {
            if (hb.OverCollider && hb.ThisPreviewEnabled)
                return true;
        }

        return false;
    }
}
