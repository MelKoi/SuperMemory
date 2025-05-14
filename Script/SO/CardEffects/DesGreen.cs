using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "战技卡销毁", menuName = "Card Effects/回合阶段禁用/战技失效")]
public class DesGreen : CardEffectAsset
{
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, bool isCounterCare)
    {
        User = GetUser(battleManager, isCounterCare);
        Used = GetUsed(battleManager);
        if (Used == battleManager.Player)
            if(battleManager.SkillArea.GetChild(0) !=  null)
            {
                Destroy(battleManager.SkillArea.GetChild(0));
                battleManager.SkillEffect.Clear();
            }    
        else
            {
                if (enemyManager.SkillArea.GetChild(0) != null)
                {
                    Destroy(enemyManager.SkillArea.GetChild(0));
                    enemyManager.SkillEffect.Clear();
                }
            }
    }
}
