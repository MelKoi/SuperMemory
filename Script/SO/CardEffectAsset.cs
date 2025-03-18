using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardEffectAsset : ScriptableObject
{
    public abstract void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user);
}
[CreateAssetMenu(fileName = "���Ա䶯Ч��", menuName = "Card Effects/���ԼӼ�")]
public class AttributeModifierEffect : CardEffectAsset
{
    public enum AttributeType { Hp, Sp, Mp, Weapon1Acc, Weapon2Acc }
    public AttributeType TargetAttribute;
    public int ModifierValue;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        // ʵ��Ч���߼�
        switch (TargetAttribute)
        {
            case AttributeType.Hp:
                user.hp += ModifierValue;
                break;
            case AttributeType.Sp:
                user.NowSp += ModifierValue;
                break;
            case AttributeType.Mp:
                user.mp += ModifierValue;
                break;
        }

        // ����UI
        if (user == battleManager.Player)
        {
            battleManager.UpdateUI(battleManager.HpText, battleManager.MpText, battleManager.SpText,
                battleManager.Weapon1Acc, battleManager.Weapon2Acc, user);
        }
        else
        {
            battleManager.UpdateUI(enemyManager.HpText, enemyManager.MpText, enemyManager.SpText,
                enemyManager.Weapon1Acc, enemyManager.Weapon2Acc, user);
        }
    }
}
[CreateAssetMenu(fileName = "��������", menuName = "Card Effects/���ٿ�������")]
public class CostReductionEffect : CardEffectAsset
{
    public int ReductionAmount;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        // ��һ��ȫ�ֱ����洢��ʱ���ļ���
        user.TemporaryCostReduction = ReductionAmount;
    }
}
[CreateAssetMenu(fileName = "�غϽ׶ν���", menuName = "Card Effects/�غϽ׶ν���")]
public class DisableActionEffect : CardEffectAsset
{
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        switch(battleManager._currentPhase)
        {
            case GamePhase.playerReady:
                battleManager._currentPhase = GamePhase.playerAction;
                battleManager.EndTurn();
                break;
            case GamePhase.playerAction:
                battleManager.EndTurn();
                break;
            case GamePhase.enemyReady:
                battleManager._currentPhase = GamePhase.playerAction;
                battleManager.EndTurn();
                break;
            case GamePhase.enemyAction:
                battleManager.EndTurn();
                break;
        }
    }
}
[CreateAssetMenu(fileName = "����", menuName = "Card Effects/����")]
public class DiscardCardEffect : CardEffectAsset
{
    public int CardNum;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        // �������һ������
        for(int i = 0; i < CardNum; i++)
            if (user == battleManager.Player)
            {
                if (battleManager.HandArea.transform.childCount > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, battleManager.HandArea.transform.childCount);
                    GameObject cardToDiscard = battleManager.HandArea.transform.GetChild(randomIndex).gameObject;
                    Destroy(cardToDiscard);
                }
            }
            else
            {
                if (enemyManager.HandArea.transform.childCount > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, enemyManager.HandArea.transform.childCount);
                    GameObject cardToDiscard = enemyManager.HandArea.transform.GetChild(randomIndex).gameObject;
                    Destroy(cardToDiscard);
                }
            }
    }
}
[CreateAssetMenu(fileName = "����", menuName = "Card Effects/����")]
public class DrewCardEffect : CardEffectAsset
{
    public int CardNum;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        for(int i = 0; i < CardNum; i++)
        {
            
        }
    }
}