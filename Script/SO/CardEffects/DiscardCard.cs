using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "弃牌", menuName = "Card Effects/弃牌")]
public class DiscardCard : CardEffectAsset
{
    public int CardNum;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        if (Used == battleManager.Player)
        {
            if (battleManager.HandArea.transform.childCount < CardNum)
                if (battleManager._currentPhase == GamePhase.playerAction || battleManager._currentPhase == GamePhase.playerReady)
                   return;//主动弃牌的效果则无效
                else
                    CardNum = battleManager.HandArea.transform.childCount;//如果是被动弃牌则有多少弃多少
        } 
        else if (Used == battleManager.Enemy)
            if (enemyManager.HandArea.transform.childCount < CardNum)
                if (battleManager._currentPhase == GamePhase.enemyAction || battleManager._currentPhase == GamePhase.enemyReady)
                    return;//主动弃牌的效果则无效
                else
                    CardNum = enemyManager.HandArea.transform.childCount;//如果是被动弃牌则有多少弃多少
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
