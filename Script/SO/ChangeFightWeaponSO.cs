using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/ChangeFightWeaponSO")]
public class ChangeFightWeaponSO : ScriptableObject
{
    public UnityAction<WeaponAsset> ChangeFightWeaponEvent;

    public void RaiseChangeFightWeaponEvent(WeaponAsset weaponAsset)
    {
        ChangeFightWeaponEvent?.Invoke(weaponAsset);
    }
}
