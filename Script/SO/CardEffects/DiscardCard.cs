using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ÆúÅÆ", menuName = "Card Effects/ÆúÅÆ")]
public class DiscardCard : CardEffectAsset
{
    public int CardNum;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager)
    {
        User = GetUser(battleManager);
        Used = GetUsed(battleManager);
        // Ëæ»úÆúÖÃÒ»ÕÅÊÖÅÆ
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
