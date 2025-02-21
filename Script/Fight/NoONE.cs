using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class NoONE : BattleManager
{
    // Start is called before the first frame update
    void Start()
    {
        GameStart();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentPhase)
        {
            case GamePhase.playerReady:
                PlayerReady();
                break;
            case GamePhase.playerAction:

                break;
            case GamePhase.enemyReady:
                EnemyReady();
                break;
            case GamePhase.enemyAction:
                StartEnemyTurn();
                break;
        }
        if(Player.Weapon1Acc != int.Parse(Weapon1Acc.text) || Player.Weapon2Acc != int.Parse(Weapon1Acc.text))
        {
            UpdateUI(HpText, MpText, SpText, Weapon1Acc, Weapon2Acc, Player);
        }
        if(Enemy.Weapon1Acc != int.Parse(EnemyManager.Weapon1Acc.text) || Enemy.Weapon2Acc != int.Parse(EnemyManager.Weapon1Acc.text))
        {
            UpdateUI(EnemyManager.HpText, EnemyManager.MpText, EnemyManager.SpText, EnemyManager.Weapon1Acc,
            EnemyManager.Weapon2Acc, Enemy);
        }
    }
    private void StartEnemyTurn()//敌人的主要阶段
    {
        Debug.Log(_currentPhase);
        EndTurn();
        //实现敌方逻辑
    }
}
