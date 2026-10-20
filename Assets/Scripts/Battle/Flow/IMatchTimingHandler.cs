public interface IMatchTimingHandler
{
    void OnMatchTiming(MatchFlow flow, MatchTiming timing);

    void OnAttackTiming(MatchFlow flow, AttackContext attack, AttackTiming timing);
}

public sealed class NullMatchTimingHandler : IMatchTimingHandler
{
    public static readonly NullMatchTimingHandler Instance = new NullMatchTimingHandler();

    private NullMatchTimingHandler()
    {
    }

    public void OnMatchTiming(MatchFlow flow, MatchTiming timing)
    {
    }

    public void OnAttackTiming(MatchFlow flow, AttackContext attack, AttackTiming timing)
    {
    }
}
