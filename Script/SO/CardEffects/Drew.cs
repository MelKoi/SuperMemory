using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "抽牌", menuName = "Card Effects/抽牌")]
public class Drew : CardEffectAsset
{
    public int DrewNum;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, bool isCounterCare)
    {
        User = GetUser(battleManager, isCounterCare);
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
