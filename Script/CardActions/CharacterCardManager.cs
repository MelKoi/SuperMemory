using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class CharacterCardManager : MonoBehaviour
{
    public CharactorAsset charactorasset;
    [Header("�ı���")]
    public Text nameText;

    [Header("ͼƬ")]
    public Image BianKuang;
    public Image CardFace;
    void Awake()
    {
        if (charactorasset != null)//���ƴ���
        {
            ReadCardFromAsset(charactorasset);
        }
    }
    private bool canBePlayedNow = false;//�Ƿ���Ա�ʹ��
    public bool CanBePlayedNow
    {
        get
        {
            return canBePlayedNow;
        }
        set
        {
            canBePlayedNow = value;

        }
    }
    public void ReadCardFromAsset(CharactorAsset charactor)
    {
        nameText.text = charactor.Cardname;

        BianKuang.sprite = charactor.BianKuang;

        CardFace.sprite = charactor.HeroImage;

    }
}
