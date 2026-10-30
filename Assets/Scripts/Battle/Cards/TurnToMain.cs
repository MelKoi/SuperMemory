using System;

// 回合开始效果之后，一直走到主要阶段之前。
// 牌库还有牌时不支付生命、不洗牌。牌库空了就把弃牌洗回牌库。
// 韧性回复没有正式数值，这一步不改韧性。
public static class TurnToMain
{
    public static Arrival Advance(FighterState active, FighterState other, CardCatalog cards, Random random)
    {
        if (active == null)
        {
            throw new ArgumentNullException(nameof(active));
        }

        if (other == null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        Tick(active);
        Tick(other);
        var expired = Expire(active, cards) + Expire(other, cards);
        var required = active.Deck.Count == 0;
        var shuffled = false;
        if (required)
        {
            active.Deck.AddRange(active.Discard);
            active.Discard.Clear();
            OpeningHand.Shuffle(active, random);
            shuffled = true;
        }

        active.Energy += MatchTimingPolicy.TurnEnergyGain;
        return new Arrival(expired, shuffled, required);
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

    public static int Expire(FighterState fighter, CardCatalog cards)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        var removed = 0;
        for (var i = fighter.Field.Count - 1; i >= 0; i--)
        {
            var card = fighter.Field[i];
            if (card.Duration > 0)
            {
                continue;
            }

            var endEffect = cards.Get(CardKind.Buff, card.EffectCardId).EndEffect;
            if (!IsEmpty(endEffect))
            {
                throw new InvalidOperationException("这张牌的弃置时效果还不能在这里结算。");
            }

            fighter.Field.RemoveAt(i);
            fighter.Discard.Add(card);
            removed++;
        }

        return removed;
    }

    static bool IsEmpty(int[][] effects)
    {
        if (effects == null)
        {
            return true;
        }

        for (var i = 0; i < effects.Length; i++)
        {
            var step = effects[i];
            if (step != null && step.Length > 0 && step[0] != CardEffects.None)
            {
                return false;
            }
        }

        return true;
    }

    public sealed class Arrival
    {
        public Arrival(int expired, bool shuffled, bool shuffleRequired)
        {
            Expired = expired;
            Shuffled = shuffled;
            ShuffleRequired = shuffleRequired;
        }

        public int Expired { get; }
        public bool Shuffled { get; }
        public bool ShuffleRequired { get; }
    }
}
