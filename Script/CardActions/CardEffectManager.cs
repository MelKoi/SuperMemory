using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    private static CardEffectManager instance;
    public static CardEffectManager Instance => instance;
   
    private List<DelayedCardEffect> delayedEffects = new List<DelayedCardEffect>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //添加延迟效果
    public void AddDelayedEffect(DelayedCardEffect effect)
    {
        delayedEffects.Add(effect);
        Debug.Log($"添加延迟效果：{effect.effectName}");
    }

    // 触发所有延迟效果
    public void TirggerDelayedEffects()
    {
        if (delayedEffects.Count == 0) return;

        foreach (var effect in delayedEffects)
        {
            Debug.Log($"触发延迟效果：{effect.effectName}");
            ApplyEffect(effect);
        }
        delayedEffects.Clear(); // 清空效果列表
    }

    // 应用效果
    private void ApplyEffect(DelayedCardEffect effect)
    {
       
    }
}
