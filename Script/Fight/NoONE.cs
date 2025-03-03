using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class NoONE : BattleManager
{
    override public void StartEnemyTurn()//���˵���Ҫ�׶�
    {
        Debug.Log(_currentPhase);
        int Behavior = 0;
        int Damage = 0;//�����˺�
        int acc = 0;// ������ʱ����
        while(Behavior < 2)
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
                Debug.Log("���ҷ����" + Damage + "���˺���");
                acc = 0;
                Damage = 0;
                continue;
            }
            //�����ƿ��ڵ�����
            Transform[] EnemyCards = EnemyManager.HandArea.transform.GetComponentsInChildren<Transform>();
            foreach(var card in EnemyCards)
            {
                CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                if (cardAsset.Type == Type.����)//������ֱ��ʹ��
                {
                    UseCard(cardAsset, card.gameObject);
                    Behavior++;
                    continue;
                }
            }
            EnemyManager.EnemyAcc();//�����������ܣ�����ֵ��Ϊ��ʱ�������������������ѡ������ֵ���ߵ�����
            Behavior++;
            if (Enemy.Weapon1 == true || Enemy.Weapon2 == true)//������������֮һ������ʹ�÷ǹ����ƣ�ʹ�������еĵ�һ��
            {
                foreach (var card in EnemyCards)
                {
                    CardAsset cardAsset = card.GetComponent<OneCardManager>().cardAsset;
                    if (cardAsset.Type != Type.����)//������ֱ��ʹ��
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
        //ʵ�ֵз��߼�
        //����ʱ���ݶ��߼��������ƻ�����ʹ�ã�û�й�����ʱ���Ƚ������ܣ����ܻ�ѡ��ǰ����ֵ���ߵ�����
        //�����ﵽ1�����������������������ڱ��غ�ʹ�ù���������ʱ���Żῼ��ʹ�÷ǹ����ƣ�
        //һ�غϽ��������ж�
        //������з�����Ӧ
    }
}
