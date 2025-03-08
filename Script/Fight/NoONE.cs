using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class NoONE : BattleManager
{
    override public void StartEnemyTurn()//敌人的主要阶段
    {
        Debug.Log(_currentPhase);
        int Behavior = 0;
        int Damage = 0;//攻击伤害
        int acc = 0;// 蓄能临时储存
        while(Behavior <= 2)
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
                {
                    Player.hp = Player.hp - Damage;
                    Debug.Log("对我方造成" + Damage + "点伤害！");
                    Enemy.mp = Enemy.mp + acc;
                    acc = Enemy.Weapon1Acc = 0;
                    Damage = 0;
                    Enemy.Weapon1 = true;
                    Behavior++;
                    continue;
                }
                    
            }
            if(Enemy.Weapon2 == false)
            {
                acc = Enemy.Weapon2Acc;
                foreach (var damage in EnemyManager._PlayerWeapons[1].Accumulation)
                {
                    if (acc >= damage.Acc)
                    {
                        Damage = damage.Value;
                    }
                }
                if (Damage != 0)
                {
                    Player.hp = Player.hp - Damage;
                    Debug.Log("对我方造成" + Damage + "点伤害！");
                    Enemy.mp = Enemy.mp + acc;
                    acc = Enemy.Weapon2Acc = 0;
                    Damage = 0;
                    Enemy.Weapon2 = true;
                    Behavior++;
                    continue;
                }
                   
            }
            //检索牌库内的手牌
            List<Transform> EnemyCards = new List<Transform>();
            Transform parent = EnemyManager.HandArea.transform;
            if(parent.childCount == 0)
            {
                Behavior++;
                continue;
            }
            for(int i = 0; i < parent.childCount; i++)
            {
                EnemyCards.Add(parent.GetChild(i));
            }
            foreach(var card in EnemyCards)
            {
                CardAsset cardAsset = card.gameObject.GetComponent<OneCardManager>().cardAsset;
                if (cardAsset.Type == Type.攻击)//攻击牌直接使用
                {
                    UseCard(cardAsset, card.gameObject, Enemy);
                    Behavior++;
                    continue;
                }
            }
            EnemyAcc(parent.GetChild(0).gameObject);//否则优先蓄能，蓄能值都为零时，两把武器随机，否则选择蓄能值更高的武器
            Behavior++;
            if (Enemy.Weapon1 == true || Enemy.Weapon2 == true)//两把武器其中之一攻击后使用非攻击牌，使用手牌中的第一张
            {
                foreach (var card in EnemyCards)
                {
                    CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                    if (cardAsset.Type != Type.攻击)//攻击牌直接使用
                    {
                        UseCard(cardAsset, card.gameObject, Enemy);
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
    override public void EnemyAcc(GameObject Card)//敌方蓄能函数
                          //在两把武器蓄能都为0，或者相同时取随机数（乐）
                          //当两把武器蓄能不同时，优先对蓄能等级高的武器进行蓄能。
    {
        if (int.Parse(EnemyManager.Weapon1Acc.text)
            == int.Parse(EnemyManager.Weapon2Acc.text))
        {
            int Num = Random.Range(1, 3);//生成1到2之间的随机整数
            if(Num == 1)
                Enemy.Weapon1Acc++;
            else if(Num == 2)
                Enemy.Weapon2Acc++;
        }
        else
        {
            if(int.Parse(EnemyManager.Weapon1Acc.text) 
                > int.Parse(EnemyManager.Weapon2Acc.text))
                Enemy.Weapon1Acc++;
            else
                Enemy.Weapon2Acc++;
        }
        Destroy(Card);
    }
}
