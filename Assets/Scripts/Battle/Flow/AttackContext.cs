using System.Collections.Generic;

public sealed class AttackContext
{
    private readonly List<AttackStepRecord> steps = new List<AttackStepRecord>();

    internal AttackContext(bool canRespond, bool comboAtDeclaration)
    {
        CanRespond = canRespond;
        ComboAtDeclaration = comboAtDeclaration;
        ConsumesAttackCount = true;
    }

    public bool CanRespond { get; }
    public bool ComboAtDeclaration { get; }
    public AttackTiming Timing { get; private set; }
    public bool Negated { get; set; }
    public bool ConsumesAttackCount { get; set; }
    public bool Aborted { get; private set; }
    public int WeaponSlot { get; set; } = -1;
    public int Distance { get; set; }
    public int LifeDamage { get; set; }
    public int ToughnessDamage { get; set; }
    public int Range { get; set; }
    public bool BlockResponses { get; set; }
    public bool WaitingForResponses { get; private set; }
    public int ResponseCount { get; private set; }
    internal int Index { get; set; }

    public bool DealsDamage => !Negated || MatchTimingPolicy.NegatedAttackDealsDamage;
    public bool AppliesLingerBonus => !Negated || MatchTimingPolicy.NegatedAttackAppliesLinger;
    public bool ConsumesLinger => !Negated || MatchTimingPolicy.NegatedAttackConsumesLinger;
    public bool SpendsCharge => !Negated || MatchTimingPolicy.NegatedAttackSpendsCharge;
    public bool SpendsAttackCount => ConsumesAttackCount && (!Negated || MatchTimingPolicy.NegatedAttackSpendsAttackCount);
    public bool CountsForTrait => !Aborted && (!Negated || MatchTimingPolicy.NegatedAttackCountsForTrait);

    public IReadOnlyList<AttackStepRecord> Steps => steps;

    internal void MarkAborted()
    {
        Aborted = true;
    }

    internal void SetWaitingForResponses(bool waiting)
    {
        WaitingForResponses = waiting;
    }

    internal void AddResponse()
    {
        ResponseCount++;
    }

    internal void AddStep(AttackTiming timing, bool skipped)
    {
        Timing = timing;
        steps.Add(new AttackStepRecord(timing, skipped));
    }
}
