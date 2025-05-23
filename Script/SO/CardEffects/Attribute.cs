using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "���Ա䶯Ч��", menuName = "Card Effects/���ԼӼ�")]
public class Attribute : CardEffectAsset
{
    public enum AttributeType { Hp, Sp, Mp, Weapon1Acc, Weapon2Acc, WeaponAcc, Damage }
    public AttributeType TargetAttribute;
    public int ModifierValue;
    public int DamageChange;//�˺���ת�䣬0ʱȫ��������1�Ƿ�����2�Ǽ��룬3������value����ֵ
    public int AccChangeMode;//���ܱ仯��ģʽ��1ΪС�ڣ�2Ϊ����

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, bool isCounterCare)
    {
        User = GetUser(battleManager, isCounterCare);
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
            case AttributeType.WeaponAcc:
                int ChangeAcc = 0;
                if(AccChangeMode == 1)
                    ChangeAcc = Used.Weapon1Acc < Used.Weapon2Acc? 1 : 2;//���ý�С����Ϊ��Ҫ�޸Ķ���1��ʾWeapon1��2��ʾWeapon2
                else if(AccChangeMode == 2)
                    ChangeAcc = Used.Weapon1Acc < Used.Weapon2Acc ? 2 : 1;//���ýϴ����Ϊ��Ҫ�޸Ķ���1��ʾWeapon1��2��ʾWeapon2
                else if(AccChangeMode == 3)//���ڹ����ƣ����ڼ�⵱ǰ�����������������ƺ󣬲�����ǰ�����Ƶ�����
                {
                    ChangeAcc = battleManager.LastAttWeapon;
                }
                switch (ChangeAcc)
                {
                    case 1:
                        Used.Weapon1Acc += ModifierValue;
                        if (Used.Weapon1Acc < 0)
                            Used.Weapon1Acc = 0;
                        break;
                    case 2:
                        Used.Weapon2Acc += ModifierValue;
                        if (Used.Weapon2Acc < 0)
                            Used.Weapon2Acc = 0;
                        break;
                }
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
