using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MyBag", menuName = "New Bag", order = 5)]
public class BagAsset : ScriptableObject
{
    [Header("默认配置数据")]
    public List<CharactorAsset> characters;
    public List<bool> charactorOwned;
    public List<WeaponAsset> weapons;
    public List<bool> weaponsOwned;
    public List<ItemAsset> items;
    public List<bool> itemsOwned;

    //获取角色
    public void SetCharactor(CharactorAsset charactor)
    {
        if (characters.Contains(charactor))
        { 
            charactorOwned[characters.IndexOf(charactor)] = true;
        }
    }
    //获取武器
    public void SetWeapon(WeaponAsset weapon)
    {
        if (weapons.Contains(weapon))
        {
            weaponsOwned[weapons.IndexOf(weapon)] = true;
        }
    }
    //获取道具
    public void SetItem(ItemAsset item) 
    {
        if (items.Contains(item))
        {
            itemsOwned[itemsOwned.IndexOf(item)] = true;
        }
    }
    //获取角色状态
    public bool GetCharactor(CharactorAsset charactor)
    {
        return charactorOwned[characters.IndexOf(charactor)];
    }
    //获取武器状态
    public bool GetWeapon(WeaponAsset weapon)
    {
        return weaponsOwned[weapons.IndexOf(weapon)];
    }
    //获取道具状态
    public bool GetItem(ItemAsset item)
    {
        return itemsOwned[itemsOwned.IndexOf(item)];
    }


    //public BagData runtimeBagData;//运行时修改的数据

    //private string savePath => Path.Combine(Application.persistentDataPath, "Bag_Data.json");

    ////初始化背包.asset数据
    //public void InitializeRuntimeData()
    //{
    //    runtimeBagData = new BagData
    //    {
    //        characterStates = new List<AssetState<CharactorAsset>>(),
    //        weaponStates = new List<AssetState<WeaponAsset>>(),
    //        itemStates = new List<AssetState<ItemAsset>>()
    //    };

    //    // 初始化默认状态
    //    foreach (var charAsset in Characters.Where(a => a != null))
    //    {
    //        runtimeBagData.characterStates.Add(new AssetState<CharactorAsset>
    //        {
    //            asset = charAsset,
    //            owned = false
    //        });
    //    }

    //    foreach (var weaponAsset in Weapons.Where(a => a != null))
    //    {
    //        runtimeBagData.weaponStates.Add(new AssetState<WeaponAsset>
    //        {
    //            asset = weaponAsset,
    //            owned = false
    //        });
    //    }
    //    foreach (var itemAsset in Items.Where(a => a != null))
    //    {
    //        runtimeBagData.itemStates.Add(new AssetState<ItemAsset>
    //        {
    //            asset = itemAsset,
    //            owned = false
    //        });
    //    }
    //}

    //// 加载保存的数据
    //public void LoadData()
    //{
    //    if (File.Exists(savePath))
    //    {
    //        string jsonData = File.ReadAllText(savePath);
    //        runtimeBagData = JsonUtility.FromJson<BagData>(jsonData);

    //        // 验证数据完整性
    //        ValidateAndUpdateData();
    //    }
    //    else
    //    {
    //        InitializeRuntimeData();
    //        SaveData();
    //    }
    //}

    //// 保存数据到文件
    //public void SaveData()
    //{
    //    string jsonData = JsonUtility.ToJson(runtimeBagData, true);
    //    File.WriteAllText(savePath, jsonData);
    //}

    //private void ValidateAndUpdateData()
    //{
    //    // 清理空引用
    //    runtimeBagData.characterStates.RemoveAll(x => x.asset == null);
    //    runtimeBagData.weaponStates.RemoveAll(x => x.asset == null);
    //    // 检查新增角色
    //    foreach (var newChar in Characters)
    //    {
    //        if (!runtimeBagData.characterStates.Exists(x => x.asset == newChar))
    //        {
    //            runtimeBagData.characterStates.Add(new AssetState<CharactorAsset>
    //            {
    //                asset = newChar,
    //                owned = false
    //            });
    //        }
    //    }
    //    //检查新增武器
    //    foreach (var newWeapon in Weapons)
    //    {
    //        if (!runtimeBagData.weaponStates.Exists(x => x.asset == newWeapon))
    //        {
    //            runtimeBagData.weaponStates.Add(new AssetState<WeaponAsset>
    //            {
    //                asset = newWeapon,
    //                owned = false
    //            });
    //        }
    //    }

    //    //检查新增道具
    //    foreach (var newItem in Items)
    //    {
    //        if (!runtimeBagData.itemStates.Exists(x => x.asset == newItem))
    //        {
    //            runtimeBagData.itemStates.Add(new AssetState<ItemAsset>
    //            {
    //                asset = newItem,
    //                owned = false
    //            });
    //        }
    //    }
    //}

}
