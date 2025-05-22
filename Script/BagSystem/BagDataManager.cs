using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagDataManager : MonoBehaviour
{
    [Header("背包")]
    public BagAsset bagAsset;

    [Header("事件监听")]
    public ChangeFightCharactorSO changeFightCharactorEvent;
    public ChangeFightWeaponSO changeFightWeaponEvent;
    [Header("当前装备")]
    public CharactorAsset fightCharactor;
    public List<WeaponAsset> fightWeapons;
    public List<WeaponAsset> startWeapons;
    public PlayerAsset Player;//玩家
    private int currentChooseWeapon = 0;
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    private void OnEnable()
    {
        changeFightCharactorEvent.ChangeFightCharactorEvent += ChangeFightCharactor;
        changeFightWeaponEvent.ChangeFightWeaponEvent += ChangeFightWeapon;
    }
    private void OnDisable()
    {
        changeFightCharactorEvent.ChangeFightCharactorEvent -= ChangeFightCharactor;
        changeFightWeaponEvent.ChangeFightWeaponEvent -= ChangeFightWeapon;
    }
    private void Start()
    {
        Player.CharacterAsset = fightCharactor;
        for (int i = 0; i < 3; i++)
        {
            Player.WeaponAsset[i] = fightWeapons[i];
        }
        AcquireCharactor(fightCharactor);
        foreach (var weapon in startWeapons)
        {
            AcquireWeapon(weapon);
        }
    }

    //武器未获取时将武器设为true
    public void AcquireWeapon(WeaponAsset weapon)
    {
        bagAsset.SetWeapon(weapon);
    }

    //查看武器获取状态
    public bool HasWeapon(WeaponAsset weapon)
    {
        return bagAsset.GetWeapon(weapon);
    }

    //人物未获取时将武器设为true
    public void AcquireCharactor(CharactorAsset charactor)
    {
        bagAsset.SetCharactor(charactor);
    }

    //查看人物获取状态
    public bool HasCharactor(CharactorAsset charactor)
    {
        return bagAsset.GetCharactor(charactor);
    }

    //武器未获取时将武器设为true
    public void AcquireItem(ItemAsset item)
    {
        bagAsset.SetItem(item);
    }

    //查看武器获取状态
    public bool HasItem(ItemAsset item)
    {
        return bagAsset.GetItem(item);
    }

    //切换角色
    public void ChangeFightCharactor(CharactorAsset charactor)
    {
        fightCharactor = charactor;
        Player.CharacterAsset = fightCharactor;
    }

    //切换武器
    public void ChangeFightWeapon(WeaponAsset weapon)
    {
        if (!fightWeapons.Contains(weapon))
        {
            fightWeapons[currentChooseWeapon] = weapon;
            for (int i = 0; i < 3; i++) {
                Player.WeaponAsset[i] = fightWeapons[i];
            }
            
            currentChooseWeapon++;
            currentChooseWeapon %= 3;
        }
    }
}
