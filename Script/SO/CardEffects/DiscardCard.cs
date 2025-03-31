using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "����", menuName = "Card Effects/����")]
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
                   return;//�������Ƶ�Ч������Ч
                else
                    CardNum = battleManager.HandArea.transform.childCount;//����Ǳ����������ж���������
        } 
        else if (Used == battleManager.Enemy)
            if (enemyManager.HandArea.transform.childCount < CardNum)
                if (battleManager._currentPhase == GamePhase.enemyAction || battleManager._currentPhase == GamePhase.enemyReady)
                    return;//�������Ƶ�Ч������Ч
                else
                    CardNum = enemyManager.HandArea.transform.childCount;//����Ǳ����������ж���������
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
