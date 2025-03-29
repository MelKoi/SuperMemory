using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneCardManager : MonoBehaviour
{
    public CardAsset cardAsset;//引用卡牌
    [Header("Text Compoment References")]//文本框
    public Text nameText;
    public Text CostText;
    public Text DescriptionText;

    [Header("Image References")]//图片
    public Image CardPic;
    public Image CardType;
    public Image CardImage;
    public GameObject CardBack;
    void Awake()
    {
        if(cardAsset != null)//卡牌存在
        {
            ReadCardFromAsset(cardAsset);
        }
    }
    private bool canBePlayedNow = false;//是否可以被使用
    public bool CanBePlayedNow
    {
        get {
            return canBePlayedNow;
        }
        set
        {
            canBePlayedNow = value;

        }
    }
    public void ReadCardFromAsset(CardAsset cardAsset)
    {
        CardType.sprite = cardAsset.TypePic;

        nameText.text = cardAsset.CardName;

        CostText.text = cardAsset.cost;

        DescriptionText.text = cardAsset.description;

        CardPic.sprite = cardAsset.WeaponAsset.CardFace;

    }
}
