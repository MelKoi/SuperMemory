using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MyBag", menuName = "New Bag", order = 5)]
public class BagAsset : ScriptableObject
{
    [Header("默认配置数据")]
    public List<CharactorAsset> initialCharacters;
    public List<WeaponAsset> initialWeapons;

    public BagData runtimeBagData;//运行时修改的数据

    private string savePath => Path.Combine(Application.persistentDataPath, "Bag_Data.json");

    //初始化背包.asset数据
    public void InitializeRuntimeData()
    {
        runtimeBagData = new BagData
        {
            characterStates = new List<AssetState<CharactorAsset>>(),
            weaponStates = new List<AssetState<WeaponAsset>>()
        };

        // 初始化默认状态
        foreach (var charAsset in initialCharacters.Where(a => a != null))
        {
            runtimeBagData.characterStates.Add(new AssetState<CharactorAsset>
            {
                asset = charAsset,
                owned = false
            });
        }

        foreach (var weaponAsset in initialWeapons.Where(a => a != null))
        {
            runtimeBagData.weaponStates.Add(new AssetState<WeaponAsset>
            {
                asset = weaponAsset,
                owned = false
            });
        }
    }

    // 加载保存的数据
    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string jsonData = File.ReadAllText(savePath);
            runtimeBagData = JsonUtility.FromJson<BagData>(jsonData);

            // 验证数据完整性
            ValidateAndUpdateData();
        }
        else
        {
            InitializeRuntimeData();
            SaveData();
        }
    }

    // 保存数据到文件
    public void SaveData()
    {
        string jsonData = JsonUtility.ToJson(runtimeBagData, true);
        File.WriteAllText(savePath, jsonData);
    }

    private void ValidateAndUpdateData()
    {
        // 清理空引用
        runtimeBagData.characterStates.RemoveAll(x => x.asset == null);
        runtimeBagData.weaponStates.RemoveAll(x => x.asset == null);
        // 检查新增角色
        foreach (var newChar in initialCharacters)
        {
            if (!runtimeBagData.characterStates.Exists(x => x.asset == newChar))
            {
                runtimeBagData.characterStates.Add(new AssetState<CharactorAsset>
                {
                    asset = newChar,
                    owned = false
                });
            }
        }
        //检查新增武器
        foreach (var newWeapon in initialWeapons)
        {
            if (!runtimeBagData.weaponStates.Exists(x => x.asset == newWeapon))
            {
                runtimeBagData.weaponStates.Add(new AssetState<WeaponAsset>
                {
                    asset = newWeapon,
                    owned = false
                });
            }
        }
    }
}
