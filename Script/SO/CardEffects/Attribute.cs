using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "���Ա䶯Ч��", menuName = "Card Effects/���ԼӼ�")]
public class Attribute : CardEffectAsset
{
    public enum AttributeType { Hp, Sp, Mp, Weapon1Acc, Weapon2Acc }
    public AttributeType TargetAttribute;
    public int ModifierValue;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        // ʵ��Ч���߼�
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
        }
        // ����UI
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
