using System;

// 流程停住之后继续往下走，直到指定一方的主要阶段。
// 这一方牌库还有牌时，停在洗牌时点等选择。牌库空了，或不是这一方，就直接洗回或跳过。
// 不是指定一方的主要阶段直接结束。这一轮测试里对方没有行动。
public static class MatchCycle
{
    public readonly struct Stop
    {
        public Stop(bool waitingForShuffle)
        {
            WaitingForShuffle = waitingForShuffle;
        }

        public bool WaitingForShuffle { get; }
    }

    public static Stop ContinueUntil(MatchRules rules, BattleSide side, Random random)
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
                return new Stop(false);
            }

            if (flow.Timing == MatchTiming.Shuffle && flow.IsWaiting)
            {
                var fighter = rules.GetFighter(flow.ActiveSide);
                if (flow.ActiveSide == side && fighter.Deck.Count > 0)
                {
                    return new Stop(true);
                }

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

    // 主动洗牌支付 1 点生命，只洗当前牌库。不洗就保持原顺序。然后给这一方回合能量。
    public static Stop AnswerShuffle(MatchRules rules, BattleSide side, bool shuffle, Random random)
    {
        if (rules == null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var flow = rules.Flow;
        if (flow.IsFinished || flow.Timing != MatchTiming.Shuffle || !flow.IsWaiting || flow.ActiveSide != side)
        {
            throw new InvalidOperationException("当前不是这一方的洗牌时点。");
        }

        var fighter = rules.GetFighter(side);
        if (fighter.Deck.Count == 0)
        {
            throw new InvalidOperationException("牌库已经空了，这一步不能选择。");
        }

        if (shuffle)
        {
            rules.PayLife(side, MatchTimingPolicy.ShuffleLifeCost);
            if (flow.IsFinished)
            {
                return new Stop(false);
            }

            DeckOrder.Shuffle(fighter, random);
        }

        flow.ResolveShuffle(shuffle);
        TurnShuffle.GrantEnergy(fighter);
        return ContinueUntil(rules, side, random);
    }

    // 不询问。牌库空了就洗回弃牌，然后给回合能量，并离开洗牌时点。
    public static void PassShuffle(MatchRules rules, Random random)
    {
        var fighter = rules.GetFighter(rules.Flow.ActiveSide);
        var shuffled = TurnShuffle.Apply(fighter, random);
        rules.Flow.ResolveShuffle(shuffled);
    }
}
