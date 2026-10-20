using System.Collections.Generic;

public static class MatchTimingOrder
{
    private static readonly MatchTiming[] beforeMatch =
    {
        MatchTiming.BuildDeck,
        MatchTiming.Seating,
    };

    private static readonly MatchTiming[] opening =
    {
        MatchTiming.OpeningHand,
        MatchTiming.OpeningEnergy,
    };

    private static readonly MatchTiming[] turn =
    {
        MatchTiming.TurnStartEffects,
        MatchTiming.TickStatuses,
        MatchTiming.ExpireStatuses,
        MatchTiming.Shuffle,
        MatchTiming.TurnEnergy,
        MatchTiming.ToughnessRecovery,
        MatchTiming.Main,
        MatchTiming.MainEnd,
        MatchTiming.RemoveTemporary,
        MatchTiming.AdjustHand,
        MatchTiming.ClearRoundMarks,
    };

    private static readonly AttackTiming[] attack =
    {
        AttackTiming.SelectLevel,
        AttackTiming.ModifyRange,
        AttackTiming.CheckAttackCount,
        AttackTiming.LockAttack,
        AttackTiming.ResponseWindow,
        AttackTiming.ResolveResponses,
        AttackTiming.CheckNegated,
        AttackTiming.DealDamage,
        AttackTiming.AfterHit,
        AttackTiming.Spend,
        AttackTiming.CountTrait,
    };

    public static IReadOnlyList<MatchTiming> BeforeMatch => beforeMatch;
    public static IReadOnlyList<MatchTiming> Opening => opening;
    public static IReadOnlyList<MatchTiming> Turn => turn;
    public static IReadOnlyList<AttackTiming> Attack => attack;
}
