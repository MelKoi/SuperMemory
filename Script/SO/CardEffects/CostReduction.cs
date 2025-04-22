using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "��������", menuName = "Card Effects/���ٿ�������")]
public class CostReduction : CardEffectAsset
{
    public int ReductionAmount;

    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, bool isCounterCare)
    {
        User = GetUser(battleManager,isCounterCare);
        Used = GetUsed(battleManager);
        // ��һ��ȫ�ֱ����洢��ʱ���ļ���
        Used.TemporaryCostReduction = ReductionAmount;
    }
}
