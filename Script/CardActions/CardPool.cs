using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPool
{
    private Dictionary<CardAsset,Queue<CardAsset>> pool = new Dictionary<CardAsset, Queue<CardAsset>>();

    public CardAsset GetCard(CardAsset cardAsset)
    {
        if (!pool.ContainsKey(cardAsset))//����������û��
        {
            pool[cardAsset] = new Queue<CardAsset>();
        }
        if (pool[cardAsset].Count > 0)//�����
        {
            return pool[cardAsset].Dequeue();
        }
        else
        {
            //����ԭʼ����ʾ�����߿�������
            return cardAsset;
        }
    }
    public void ReturnCard(CardAsset card)
    {
        pool[card].Enqueue(card);
    }
}
