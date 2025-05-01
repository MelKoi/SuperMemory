using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "对应卡禁用", menuName = "Card Effects/回合阶段禁用/无法对应")]
public class BanPurple : CardEffectAsset
{
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, bool isCounterCare)
    {
        User = GetUser(battleManager, isCounterCare);
        Used = GetUsed(battleManager);
        if (Used == battleManager.Player)
            battleManager.Purple.GetComponent<Image>().sprite = battleManager.PClose;
        else
            enemyManager.Purple.GetComponent<Image>().sprite = battleManager.PClose;
    }
}
