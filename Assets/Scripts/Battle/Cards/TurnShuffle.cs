using System;

// 洗牌时点停住之后要补的两步：空牌库洗回弃牌，再给当前这一方回合能量。
// 不放进 TurnCardRules。直接 ResolveShuffle 的调用方自己决定要不要给能量。
public static class TurnShuffle
{
    public static bool Apply(FighterState active, Random random)
    {
        if (active == null)
        {
            throw new ArgumentNullException(nameof(active));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var shuffled = DeckOrder.RecycleIfEmpty(active, random);
        GrantEnergy(active);
        return shuffled;
    }

    public static void GrantEnergy(FighterState active)
    {
        if (active == null)
        {
            throw new ArgumentNullException(nameof(active));
        }

        GainRates.Energy(active, MatchTimingPolicy.TurnEnergyGain);
    }
}
