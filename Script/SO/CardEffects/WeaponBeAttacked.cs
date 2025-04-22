using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "设置武器",menuName = "Card Effects/武器设置")]
public class WeaponBeAttacked : CardEffectAsset
{
    public int WeaponNum;//武器序号，1为1号武器，2为2号武器，3为两把武器,4为随机
    public int WeaponCanAtk;//设置为能否攻击，1为能，0为不能
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, bool isCounterCare)
    {
        User = GetUser(battleManager, isCounterCare);
        Used = GetUsed(battleManager);
        switch(WeaponNum)
        {
            case 1:
                if(WeaponCanAtk == 1)
                    Used.Weapon1 = false;
                else
                    Used.Weapon1 = true;
                break;

            case 2:
                if (WeaponCanAtk == 1)
                    Used.Weapon2 = false;
                else
                    Used.Weapon2 = true;
                break;

            case 3:
                if (WeaponCanAtk == 1)
                {
                    Used.Weapon1 = false;
                    Used.Weapon2 = false;
                }
                else
                {
                    Used.Weapon1 = true;
                    Used.Weapon2 = true;
                }
                break;
            case 4:
                int randomIndex = Random.Range(1, 3);
                switch(randomIndex)
                {
                    case 1:
                        if (WeaponCanAtk == 1)
                            Used.Weapon1 = false;
                        else
                            Used.Weapon1 = true;
                        break;
                    case 2:
                        if (WeaponCanAtk == 1)
                            Used.Weapon2 = false;
                        else
                            Used.Weapon2 = true;
                        break;
                }
                break;
        }
    }
}
