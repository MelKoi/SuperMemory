public readonly struct MainActionRecord
{
    public MainActionRecord(int turnIndex, BattleSide side, MainActionKind kind, bool comboAlreadyActive, int traitCountAfter)
    {
        TurnIndex = turnIndex;
        Side = side;
        Kind = kind;
        ComboAlreadyActive = comboAlreadyActive;
        TraitCountAfter = traitCountAfter;
    }

    public int TurnIndex { get; }
    public BattleSide Side { get; }
    public MainActionKind Kind { get; }
    public bool ComboAlreadyActive { get; }
    public int TraitCountAfter { get; }
}
