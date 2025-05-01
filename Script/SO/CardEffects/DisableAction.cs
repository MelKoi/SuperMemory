using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ìø¹ý½×¶Î", menuName = "Card Effects/»ØºÏ½×¶Î½ûÓÃ/Ìø¹ý½×¶Î")]
public class DisableAction : CardEffectAsset
{
    public GamePhase nowGamePhase;
    public override void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager, bool isCounterCare)
    {
        User = GetUser(battleManager, isCounterCare);
        Used = GetUsed(battleManager);
        if(battleManager._currentPhase == nowGamePhase)
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
