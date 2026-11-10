using System;

// 洗牌时点停住之后要补的两步：空牌库洗回弃牌，再给当前这一方回合能量。
// 不放进 TurnCardRules。测试会在流程开始前写好能量，经过回合能量时点时不能再加一次。
public static class TurnShuffle
{
    public static bool Apply(FighterState active, Random random)
    {
        if (active == null)
        {
            throw new ArgumentNullException(nameof(active));
        }

        var shuffled = DeckOrder.RecycleIfEmpty(active, random);
        active.Energy += MatchTimingPolicy.TurnEnergyGain;
        return shuffled;
    }
}
