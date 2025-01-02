using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneCardManager : MonoBehaviour
{
    public CardAsset cardAsset;//���ÿ���
    [Header("Text Compoment References")]//�ı���
    public Text nameText;
    public Text CostText;
    public Text DescriptionText;

    [Header("Image References")]//ͼƬ
    public Image CardPic;
    public Image CardType;
    void Awake()
    {
        if(cardAsset != null)//���ƴ���
        {
            ReadCardFromAsset();
        }
    }
    private bool canBePlayedNow = false;//�Ƿ���Ա�ʹ��
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
    public void ReadCardFromAsset()
    {
        CardType.sprite = cardAsset.TypePic;

        nameText.text = cardAsset.CardName;

        CostText.text = cardAsset.cost;

        DescriptionText.text = cardAsset.description;

        CardPic.sprite = cardAsset.cardPic;

    }
}
