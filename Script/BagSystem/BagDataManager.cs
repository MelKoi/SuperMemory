using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagDataManager : MonoBehaviour
{
    [Header("背包")]
    public BagAsset bagAsset;

    private void Awake()
    {
        InitializeBagData();
    }

    //加载背包数据
    private void InitializeBagData()
    {
        if (bagAsset.runtimeBagData == null) //若未检测到背包数据则初始化背包.asset文件
        {
            bagAsset.InitializeRuntimeData();
        }
        bagAsset.LoadData();
    }

    private void OnApplicationQuit()
    {
        SaveBagData();
    }

    public void SaveBagData()
    {
        bagAsset.SaveData();
    }

    //武器未获取时将武器设为true
    public void AcquireWeapon(WeaponAsset weapon)
    {
        bagAsset.runtimeBagData.SetItemOwned(weapon, true);
        SaveBagData();
    }

    //查看武器获取状态
    public bool HasWeapon(WeaponAsset weapon)
    {
        return bagAsset.runtimeBagData.HasItem(weapon);
    }

    //人物未获取时将武器设为true
    public void AcquireCharactor(CharactorAsset charactor)
    {
        bagAsset.runtimeBagData.SetItemOwned(charactor, true);
        SaveBagData();
    }

    //查看人物获取状态
    public bool HasCharactor(CharactorAsset charactor)
    {
        return bagAsset.runtimeBagData.HasItem(charactor);
    }

    //武器未获取时将武器设为true
    public void AcquireItem(ItemAsset item)
    {
        bagAsset.runtimeBagData.SetItemOwned(item, true);
        SaveBagData();
    }

    //查看武器获取状态
    public bool HasItem(ItemAsset item)
    {
        return bagAsset.runtimeBagData.HasItem(item);
    }
}
