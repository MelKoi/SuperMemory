using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ս��������", menuName = "Card Effects/�غϽ׶ν���/ս��ʧЧ")]
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
