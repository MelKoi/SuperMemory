using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BagData
{
    public List<AssetState<CharactorAsset>> characterStates;
    public List<AssetState<WeaponAsset>> weaponStates;
    public List<AssetState<ItemAsset>> itemStates;

    public bool HasItem<T>(T asset) where T : ScriptableObject
    {
        if (typeof(T) == typeof(CharactorAsset))
        {
            var state = characterStates.Find(x => x.asset == asset as CharactorAsset);
            return state?.owned ?? false;
        }
        else if (typeof(T) == typeof(WeaponAsset))
        {
            var state = weaponStates.Find(x => x.asset == asset as WeaponAsset);
            return state?.owned ?? false;
        }else if(typeof(T) == typeof(ItemAsset))
        {
            var state = itemStates.Find(x => x.asset == asset as ItemAsset);
            return state?.owned ?? false;
        }
        return false;
    }

    public void SetItemOwned<T>(T asset, bool owned) where T : ScriptableObject
    {
        if (typeof(T) == typeof(CharactorAsset))
        {
            var state = characterStates.Find(x => x.asset == asset as CharactorAsset);
            if (state != null) state.owned = owned;
        }
        else if (typeof(T) == typeof(WeaponAsset))
        {
            var state = weaponStates.Find(x => x.asset == asset as WeaponAsset);
            if (state != null) state.owned = owned;
        }
        else if (typeof(T) == typeof(ItemAsset))
        {
            var state = itemStates.Find(x => x.asset == asset as ItemAsset);
            if (state != null) state.owned = owned;
        }
    }
}

[System.Serializable]
public class AssetState<T> where T : ScriptableObject
{
    public T asset;
    public bool owned;
}