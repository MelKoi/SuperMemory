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

    //����ӳ�Ч��
    public void AddDelayedEffect(DelayedCardEffect effect)
    {
        delayedEffects.Add(effect);
        Debug.Log($"����ӳ�Ч����{effect.effectName}");
    }

    // ���������ӳ�Ч��
    public void TirggerDelayedEffects()
    {
        if (delayedEffects.Count == 0) return;

        foreach (var effect in delayedEffects)
        {
            Debug.Log($"�����ӳ�Ч����{effect.effectName}");
            ApplyEffect(effect);
        }
        delayedEffects.Clear(); // ���Ч���б�
    }

    // Ӧ��Ч��
    private void ApplyEffect(DelayedCardEffect effect)
    {
       
    }
}
