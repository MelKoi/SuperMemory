using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class NoONE : BattleManager
{
    override public void StartEnemyTurn()//���˵���Ҫ�׶�
    {
        Debug.Log(_currentPhase);
        int Behavior = 0;
        int Damage = 0;//�����˺�
        int acc = 0;// ������ʱ����
        while(Behavior <= 2)
        {
            //��������1�����ܾ����Ϲ���
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
                    Debug.Log("���ҷ����" + Damage + "���˺���");
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
                    Debug.Log("���ҷ����" + Damage + "���˺���");
                    Enemy.mp = Enemy.mp + acc;
                    acc = Enemy.Weapon2Acc = 0;
                    Damage = 0;
                    Enemy.Weapon2 = true;
                    Behavior++;
                    continue;
                }
                   
            }
            //�����ƿ��ڵ�����
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
                if (cardAsset.Type == Type.����)//������ֱ��ʹ��
                {
                    UseCard(cardAsset, card.gameObject, Enemy);
                    Behavior++;
                    continue;
                }
            }
            EnemyAcc(parent.GetChild(0).gameObject);//�����������ܣ�����ֵ��Ϊ��ʱ�������������������ѡ������ֵ���ߵ�����
            Behavior++;
            if (Enemy.Weapon1 == true || Enemy.Weapon2 == true)//������������֮һ������ʹ�÷ǹ����ƣ�ʹ�������еĵ�һ��
            {
                foreach (var card in EnemyCards)
                {
                    CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                    if (cardAsset.Type != Type.����)//������ֱ��ʹ��
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
        //ʵ�ֵз��߼�
        //����ʱ���ݶ��߼��������ƻ�����ʹ�ã�û�й�����ʱ���Ƚ������ܣ����ܻ�ѡ��ǰ����ֵ���ߵ�����
        //�����ﵽ1�����������������������ڱ��غ�ʹ�ù���������ʱ���Żῼ��ʹ�÷ǹ����ƣ�
        //һ�غϽ��������ж�
        //������з�����Ӧ
    }
    override public void EnemyAcc(GameObject Card)//�з����ܺ���
                          //�������������ܶ�Ϊ0��������ͬʱȡ��������֣�
                          //�������������ܲ�ͬʱ�����ȶ����ܵȼ��ߵ������������ܡ�
    {
        if (int.Parse(EnemyManager.Weapon1Acc.text)
            == int.Parse(EnemyManager.Weapon2Acc.text))
        {
            int Num = Random.Range(1, 3);//����1��2֮����������
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
