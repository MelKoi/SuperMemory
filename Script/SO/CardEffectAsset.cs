using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CardEffectAsset : ScriptableObject
{
    public PlayerAsset User;//使用者
    public PlayerAsset Used;//被使用者
    public int who;//对谁使用，0为对自己，1为对敌方
    public PlayerAsset GetUser(BattleManager battleManager)
    {
        if (battleManager._currentPhase == GamePhase.playerAction)//如果是玩家使用牌
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
[CreateAssetMenu(fileName = "属性变动效果", menuName = "Card Effects/属性加减")]
public class AttributeModifierEffect : CardEffectAsset
{
    public enum AttributeType { Hp, Sp, Mp, Weapon1Acc, Weapon2Acc }
    public AttributeType TargetAttribute;
    public int ModifierValue;

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
                if(Used.NowSp < 0)
                    Used.NowSp = 0;
                break;
            case AttributeType.Mp:
                Used.mp += ModifierValue;
                if(Used.mp < 0)
                    Used.mp = 0;
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
[CreateAssetMenu(fileName = "减少消耗", menuName = "Card Effects/减少卡牌消耗")]
public class CostReductionEffect : CardEffectAsset
{
    public int ReductionAmount;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        // 有一个全局变量存储临时消耗减少
        Used.TemporaryCostReduction = ReductionAmount;
    }
}
[CreateAssetMenu(fileName = "回合阶段禁用", menuName = "Card Effects/回合阶段禁用")]
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
[CreateAssetMenu(fileName = "弃牌", menuName = "Card Effects/弃牌")]
public class DiscardCardEffect : CardEffectAsset
{
    public int CardNum;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        // 随机弃置一张手牌
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
