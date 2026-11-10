using System;

// 流程停住之后继续往下走，直到指定一方的主要阶段。
// 洗牌时点交给 TurnShuffle。不是指定一方的主要阶段直接结束。这一轮测试里对方没有行动。
public static class MatchCycle
{
    public static void ContinueUntil(MatchRules rules, BattleSide side, Random random)
    {
        if (rules == null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        for (var step = 0; step < 8; step++)
        {
            var flow = rules.Flow;
            if (flow.IsFinished)
            {
                throw new InvalidOperationException("这局已经结束。");
            }

            if (flow.Timing == MatchTiming.Main && flow.IsWaiting && flow.ActiveSide == side)
            {
                return;
            }

            if (flow.Timing == MatchTiming.Shuffle && flow.IsWaiting)
            {
                PassShuffle(rules, random);
                continue;
            }

            if (flow.Timing == MatchTiming.Main && flow.IsWaiting)
            {
                rules.EndMainPhase();
                continue;
            }

            throw new InvalidOperationException("当前时点不能继续回合。");
        }

        throw new InvalidOperationException("回合没有回到这一方的主要阶段。");
    }

    static void PassShuffle(MatchRules rules, Random random)
    {
        var fighter = rules.GetFighter(rules.Flow.ActiveSide);
        var shuffled = TurnShuffle.Apply(fighter, random);
        rules.Flow.ResolveShuffle(shuffled);
    }
}
