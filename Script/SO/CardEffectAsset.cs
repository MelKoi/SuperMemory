﻿using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class CardEffectAsset : ScriptableObject
{
    public PlayerAsset User;//使用者
    public PlayerAsset Used;//被使用者
    public int who;//对谁使用，0为对自己，1为对敌方
    public PlayerAsset GetUser(BattleManager battleManager,bool isCounterCare)//判定是否为对应牌
    {
        if(!isCounterCare)
            if (battleManager._currentPhase == GamePhase.playerAction)//如果是玩家使用牌
                return battleManager.Player;
            else
                return battleManager.Enemy;
        else
            if (battleManager._currentPhase == GamePhase.playerAction)//如果是玩家使用牌
            return battleManager.Enemy;
        else
            return battleManager.Player;
    }
    public PlayerAsset GetUsed(BattleManager battleManager)
    {
        if(who == 0)
        {
            if(User == battleManager.Player)
                return battleManager.Player;
            else
                return battleManager.Enemy;
        }
        else
        {
            if (User == battleManager.Player)
                return battleManager.Enemy;
            else
                return battleManager.Player;
            
        }
    }
    public abstract void ApplyEffect(BattleManager battleManager, EnemyManager enemyManager,bool isCounterCare);
}

