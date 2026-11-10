using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 三种大类各用一张预制体。手牌里的实例从这里取出。
public static class CardPrefabLibrary
{
    const string AttackPath = "Assets/Prefabs/Cards/AttackCard.prefab";
    const string ActionPath = "Assets/Prefabs/Cards/ActionCard.prefab";
    const string BuffPath = "Assets/Prefabs/Cards/BuffCard.prefab";

    static CardView attack;
    static CardView action;
    static CardView buff;

    public static CardView For(CardKind kind)
    {
        EnsureLoaded();
        switch (kind)
        {
            case CardKind.Attack:
                return attack;
            case CardKind.Buff:
                return buff;
            default:
                return action;
        }
    }

    static void EnsureLoaded()
    {
        if (attack != null && action != null && buff != null)
        {
            return;
        }

#if UNITY_EDITOR
        attack = attack != null ? attack : AssetDatabase.LoadAssetAtPath<CardView>(AttackPath);
        action = action != null ? action : AssetDatabase.LoadAssetAtPath<CardView>(ActionPath);
        buff = buff != null ? buff : AssetDatabase.LoadAssetAtPath<CardView>(BuffPath);
#endif
    }
}
