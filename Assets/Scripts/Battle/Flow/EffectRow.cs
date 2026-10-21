public enum EffectRow
{
    Base,
    Combo,
    Response,
}

public static class EffectRows
{
    public static EffectRow Choose(bool comboAlreadyActive, bool hasComboRow, bool responding)
    {
        if (responding)
        {
            return EffectRow.Response;
        }

        if (MatchTimingPolicy.ComboRowReplacesBaseRow && comboAlreadyActive && hasComboRow)
        {
            return EffectRow.Combo;
        }

        return EffectRow.Base;
    }
}
