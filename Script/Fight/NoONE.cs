using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class NoONE : BattleManager
{
    override public void StartEnemyTurn()//敌人的主要阶段
    {
        Debug.Log(_currentPhase);
        int Behavior = 0;
        int Damage = 0;//攻击伤害
        int acc = 0;// 蓄能临时储存
        while(Behavior < 2)
        {
            //武器到达1级蓄能就马上攻击
            if(Enemy.Weapon1 == false)
            {
                acc = Enemy.Weapon1Acc;
                foreach (var damage in EnemyManager._PlayerWeapons[0].Accumulation)
                {
                    if (acc >= damage.Acc)
                    {
                        Damage = damage.Value;
                    }
                }
                if (Damage != 0)
                    Enemy.Weapon1 = true;
            }
            else if(Enemy.Weapon2 == false)
            {
                acc = Enemy.Weapon1Acc;
                foreach (var damage in EnemyManager._PlayerWeapons[0].Accumulation)
                {
                    if (acc >= damage.Acc)
                    {
                        Damage = damage.Value;
                    }
                }
                if (Damage != 0)
                    Enemy.Weapon2 = true;
            }
            if(Player.hp != 0 && Damage != 0)
            {
                Player.hp = Player.hp - Damage;
                Debug.Log("对我方造成" + Damage + "点伤害！");
                acc = 0;
                Damage = 0;
                continue;
            }
            //检索牌库内的手牌
            Transform[] EnemyCards = EnemyManager.HandArea.transform.GetComponentsInChildren<Transform>();
            foreach(var card in EnemyCards)
            {
                CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                if (cardAsset.Type == Type.攻击)//攻击牌直接使用
                {
                    UseCard(cardAsset, card.gameObject);
                    Behavior++;
                    continue;
                }
            }
            EnemyManager.EnemyAcc();//否则优先蓄能，蓄能值都为零时，两把武器随机，否则选择蓄能值更高的武器
            Behavior++;
            if (Enemy.Weapon1 == true || Enemy.Weapon2 == true)//两把武器其中之一攻击后使用非攻击牌，使用手牌中的第一张
            {
                foreach (var card in EnemyCards)
                {
                    CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                    if (cardAsset.Type != Type.攻击)//攻击牌直接使用
                    {
                        UseCard(cardAsset, card.gameObject);
                        Behavior++;
                        continue;
                    }
                }
            }
            continue;
        }
        EndTurn();
        //实现敌方逻辑
        //测试时的暂定逻辑：攻击牌会马上使用，没有攻击牌时优先进行蓄能，蓄能会选择当前蓄能值更高的武器
        //武器达到1级蓄能则马上武器攻击，在本回合使用过武器攻击时，才会考虑使用非攻击牌，
        //一回合进行三次行动
        //不会进行翻滚对应
    }
}
