using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardEffectAsset : ScriptableObject
{
    public abstract void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user);
}
[CreateAssetMenu(fileName = "属性变动效果", menuName = "Card Effects/属性加减")]
public class AttributeModifierEffect : CardEffectAsset
{
    public enum AttributeType { Hp, Sp, Mp, Weapon1Acc, Weapon2Acc }
    public AttributeType TargetAttribute;
    public int ModifierValue;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        // 实现效果逻辑
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

        // 更新UI
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
[CreateAssetMenu(fileName = "减少消耗", menuName = "Card Effects/减少卡牌消耗")]
public class CostReductionEffect : CardEffectAsset
{
    public int ReductionAmount;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        // 有一个全局变量存储临时消耗减少
        user.TemporaryCostReduction = ReductionAmount;
    }
}
[CreateAssetMenu(fileName = "回合阶段禁用", menuName = "Card Effects/回合阶段禁用")]
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
[CreateAssetMenu(fileName = "弃牌", menuName = "Card Effects/弃牌")]
public class DiscardCardEffect : CardEffectAsset
{
    public int CardNum;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, PlayerAsset user)
    {
        // 随机弃置一张手牌
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
[CreateAssetMenu(fileName = "抽牌", menuName = "Card Effects/抽牌")]
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