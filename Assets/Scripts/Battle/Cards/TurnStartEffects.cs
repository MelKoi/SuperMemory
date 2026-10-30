using System;
using System.Collections.Generic;

// 回合开始只结算明确挂在这一时点的展开中效果。燃心的展开中在回合结束第一步，不在这里。
public static class TurnStartEffects
{
    public static bool IsBound(CardKind kind, int cardId)
    {
        return false;
    }

    public static List<CardInstance> Collect(FighterState active)
    {
        if (active == null)
        {
            throw new ArgumentNullException(nameof(active));
        }

        var bound = new List<CardInstance>();
        for (var i = 0; i < active.Field.Count; i++)
        {
            var card = active.Field[i];
            if (IsBound(card.Kind, card.EffectCardId))
            {
                bound.Add(card);
            }
        }

        return bound;
    }
}
