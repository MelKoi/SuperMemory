using System;

// 流程还没开始时，把先手从回合开始走到主要阶段之前。
// 读数用 BuffDurations。洗牌和回合能量用 TurnShuffle。韧性回复没有正式数值，这一步不改韧性。
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

        BuffDurations.TickBoth(active, other);
        var expired = BuffDurations.ExpireBoth(active, other, cards, RejectEndEffect);
        var shuffled = TurnShuffle.Apply(active, random);
        return new Arrival(expired, shuffled, shuffled);
    }

    // 这里没有效果结算，带弃置时效果的牌到期要交给 MatchRules。
    static void RejectEndEffect(FighterState fighter, int[][] endEffect)
    {
        if (!CardEffects.IsEmpty(endEffect))
        {
            throw new InvalidOperationException("这张牌的弃置时效果还不能在这里结算。");
        }
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
