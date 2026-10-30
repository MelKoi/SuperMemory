using System;

// 规则各部分共用的对局状态：牌表、两方和时点。
sealed class MatchState
{
    public MatchState(CardCatalog cards, FighterState player, FighterState enemy, IMatchTimingHandler handler)
    {
        Cards = cards ?? throw new ArgumentNullException(nameof(cards));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
        Flow = new MatchFlow(handler);
    }

    public CardCatalog Cards { get; }
    public FighterState Player { get; }
    public FighterState Enemy { get; }
    public MatchFlow Flow { get; }
    public FighterState Active => Fighter(Flow.ActiveSide);
    public FighterState Inactive => Fighter(MatchFlow.Opposite(Flow.ActiveSide));
    public Action<FighterState> KnockedOut { get; set; }

    public FighterState Fighter(BattleSide side)
    {
        return side == Player.Side ? Player : Enemy;
    }

    public FighterState Target(BattleSide source, int[] step)
    {
        return Fighter(CardEffects.TargetSide(source, step));
    }

    // 生命从有到 0 时这一方倒下。
    public void ChangeHealth(FighterState fighter, int delta)
    {
        var before = fighter.Health;
        fighter.SetHealth(before + delta);
        if (before > 0 && fighter.Health == 0)
        {
            KnockedOut?.Invoke(fighter);
        }
    }

    // 打出、蓄能、攻击都要在自己主要阶段的空闲时进行。
    public string MainActionError(BattleSide side, string notNow)
    {
        if (Flow.Timing != MatchTiming.Main || !Flow.IsWaiting || Flow.AttackInProgress)
        {
            return notNow;
        }

        if (side != Flow.ActiveSide)
        {
            return "现在不是这一方的主要阶段。";
        }

        if (Flow.PendingAllOut)
        {
            return "全力已经打出，主要阶段只能结束。";
        }

        return null;
    }
}
