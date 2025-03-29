using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCardManager : MonoBehaviour
{
    public WeaponAsset weaponAsset;//引用武器牌


    [Header("素材资源")]
    public Image WeaponCardBack;
    public Image WeaponCardFace;
    public Text WeaponNameText;
    public Text DescriptionText;
    // Start is called before the first frame update
    void Awake()
    {
        if (weaponAsset != null)//卡牌存在
        {
            ReadCardFromAsset(weaponAsset);
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
    public void ReadCardFromAsset(WeaponAsset weapon)
    {
        WeaponNameText.text = weapon.WeaponName;

        WeaponCardBack.sprite = weapon.CardBack;

        WeaponCardFace.sprite = weapon.CardFace;

        DescriptionText.text = weapon.description;

    }
}
