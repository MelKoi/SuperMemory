public readonly struct MainActionRecord
{
    public MainActionRecord(int turnIndex, BattleSide side, MainActionKind kind, int traitCountAfter)
    {
        TurnIndex = turnIndex;
        Side = side;
        Kind = kind;
        TraitCountAfter = traitCountAfter;
    }

    public int TurnIndex { get; }
    public BattleSide Side { get; }
    public MainActionKind Kind { get; }
    public int TraitCountAfter { get; }
}
