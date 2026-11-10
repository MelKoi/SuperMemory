using System;

// 加成牌的读数。回合开始时敌我场上的读数都减 1，读数到 0 的牌结算弃置时，然后离场进弃牌堆。
// 弃置时怎么结算由调用方决定。
public static class BuffDurations
{
    public static void TickBoth(FighterState active, FighterState other)
    {
        Tick(active);
        Tick(other);
    }

    public static int ExpireBoth(
        FighterState active,
        FighterState other,
        CardCatalog cards,
        Action<FighterState, int[][]> resolveEndEffect)
    {
        return Expire(active, cards, resolveEndEffect) + Expire(other, cards, resolveEndEffect);
    }

    public static void Tick(FighterState fighter)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        for (var i = 0; i < fighter.Field.Count; i++)
        {
            fighter.Field[i].Duration--;
        }
    }

    public static int Expire(FighterState fighter, CardCatalog cards, Action<FighterState, int[][]> resolveEndEffect)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        if (resolveEndEffect == null)
        {
            throw new ArgumentNullException(nameof(resolveEndEffect));
        }

        var removed = 0;
        for (var i = fighter.Field.Count - 1; i >= 0; i--)
        {
            var card = fighter.Field[i];
            if (card.Duration > 0)
            {
                continue;
            }

            resolveEndEffect(fighter, cards.Get(CardKind.Buff, card.EffectCardId).EndEffect);
            fighter.Field.RemoveAt(i);
            fighter.Discard.Add(card);
            removed++;
        }

        return removed;
    }
}
