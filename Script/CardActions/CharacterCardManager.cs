using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CharacterCardManager : MonoBehaviour
{
    public CharactorAsset charactorasset;
    [Header("文本框")]
    public Text nameText;

    [Header("图片")]
    public Image BianKuang;
    public Image CardFace;
    void Awake()
    {
        if (charactorasset != null)//卡牌存在
        {
            ReadCardFromAsset(charactorasset);
        }
    }
    private bool canBePlayedNow = false;//是否可以被使用
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
