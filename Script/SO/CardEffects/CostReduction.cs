using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "减少消耗", menuName = "Card Effects/减少卡牌消耗")]
public class CostReduction : CardEffectAsset
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
