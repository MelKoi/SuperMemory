using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWeaponManager : MonoBehaviour
{
    [Header("广播")]
    public ChangeFightWeaponSO changeFightWeaponEvent;

    public WeaponAsset weapon;
    //点击启用
    public void OnClickWeaponCard()
    {
        weapon = GetComponent<WeaponCardManager>().weaponAsset;
        changeFightWeaponEvent.RaiseChangeFightWeaponEvent(weapon);
    }
}
