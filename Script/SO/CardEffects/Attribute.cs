using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "属性变动效果", menuName = "Card Effects/属性加减")]
public class Attribute : CardEffectAsset
{
    public enum AttributeType { Hp, Sp, Mp, Weapon1Acc, Weapon2Acc, Damage }
    public AttributeType TargetAttribute;
    public int ModifierValue;
    public int DamageChange;//伤害的转变，0时全部消除，1是翻倍，2是减半，3是增加value的数值

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);    
        // 实现效果逻辑
        switch (TargetAttribute)
        {
            case AttributeType.Hp:
                Used.hp += ModifierValue;
                if (Used.hp < 0)
                    Used.hp = 0;
                break;
            case AttributeType.Sp:
                Used.NowSp += ModifierValue;
                if (Used.NowSp < 0)
                    Used.NowSp = 0;
                break;
            case AttributeType.Mp:
                Used.mp += ModifierValue;
                if (Used.mp < 0)
                    Used.mp = 0;
                break;
            case AttributeType.Damage:
                if (DamageChange == 0)
                    Used.Damage = 0;
                else if (DamageChange == 1)
                    Used.Damage = Used.Damage * 2;
                else if (DamageChange == 2)
                    Used.Damage += Used.Damage / 2;
                else if (DamageChange == 3)
                    Used.Damage += ModifierValue;
                    break;
            case AttributeType.Weapon1Acc:
                Used.Weapon1Acc += ModifierValue;
                if (Used.Weapon1Acc < 0)
                    Used.Weapon1Acc = 0;
                break;
            case AttributeType.Weapon2Acc:
                Used.Weapon2Acc += ModifierValue;
                if (Used.Weapon2Acc < 0)
                    Used.Weapon2Acc = 0;
                break;

        }
        // 更新UI
        if (Used == battleManager.Player)
        {
            battleManager.UpdateUI(battleManager.HpText, battleManager.MpText, battleManager.SpText,
                battleManager.Weapon1Acc, battleManager.Weapon2Acc, User);
        }
        else
        {
            battleManager.UpdateUI(enemyManager.HpText, enemyManager.MpText, enemyManager.SpText,
                enemyManager.Weapon1Acc, enemyManager.Weapon2Acc, User);
        }
    }
}
