public readonly struct AttackStepRecord
{
    public AttackStepRecord(AttackTiming timing, bool skipped)
    {
        Timing = timing;
        Skipped = skipped;
    }

    public AttackTiming Timing { get; }
    public bool Skipped { get; }
}
