using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPool
{
    private Dictionary<CardAsset,Queue<CardAsset>> pool = new Dictionary<CardAsset, Queue<CardAsset>>();

    public CardAsset GetCard(CardAsset cardAsset)
    {
        if (!pool.ContainsKey(cardAsset))//如果对象池中没有
        {
            pool[cardAsset] = new Queue<CardAsset>();
        }
        if (pool[cardAsset].Count > 0)//如果有
        {
            return pool[cardAsset].Dequeue();
        }
        else
        {
            //返回原始卡牌示例或者拷贝副本
            return cardAsset;
        }
    }
    public void ReturnCard(CardAsset card)
    {
        pool[card].Enqueue(card);
    }
}
