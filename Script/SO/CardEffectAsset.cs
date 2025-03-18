using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CardEffectAsset : ScriptableObject
{
    public PlayerAsset User;//ʹ����
    public PlayerAsset Used;//��ʹ����
    public int who;//��˭ʹ�ã�0Ϊ���Լ���1Ϊ�Եз�
    public PlayerAsset GetUser(BattleManager battleManager)
    {
        if (battleManager._currentPhase == GamePhase.playerAction)//��������ʹ����
            return battleManager.Player;
        else
            return battleManager.Enemy;
    }
    public PlayerAsset GetUsed(BattleManager battleManager)
    {
        if(who == 0)
        {
            if(User == battleManager.Player)
                return battleManager.Player;
            else
                return battleManager.Enemy;
        }
        else
        {
            if (User == battleManager.Player)
                return battleManager.Enemy;
            else
                return battleManager.Player;
            
        }
    }
    public abstract void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager);
}
[CreateAssetMenu(fileName = "���Ա䶯Ч��", menuName = "Card Effects/���ԼӼ�")]
public class AttributeModifierEffect : CardEffectAsset
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
                if(Used.NowSp < 0)
                    Used.NowSp = 0;
                break;
            case AttributeType.Mp:
                Used.mp += ModifierValue;
                if(Used.mp < 0)
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
[CreateAssetMenu(fileName = "��������", menuName = "Card Effects/���ٿ�������")]
public class CostReductionEffect : CardEffectAsset
{
    public int ReductionAmount;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        // ��һ��ȫ�ֱ����洢��ʱ���ļ���
        Used.TemporaryCostReduction = ReductionAmount;
    }
}
[CreateAssetMenu(fileName = "�غϽ׶ν���", menuName = "Card Effects/�غϽ׶ν���")]
public class DisableActionEffect : CardEffectAsset
{
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        switch (battleManager._currentPhase)
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
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        // �������һ������
        for (int i = 0; i < CardNum; i++)
            if (Used == battleManager.Player)
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
    public int DrewNum;
    
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        for (int i = 0; i < DrewNum; i++)
        {
            if (Used.Player == 0)
                battleManager.DrowCards(1, battleManager.HandArea, battleManager._currentDeck);
            else
                battleManager.DrowCards(1, enemyManager.HandArea, enemyManager._currentDeck);
        }
    }
}