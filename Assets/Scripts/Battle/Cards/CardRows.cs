// 连击行：持有连击且这张牌有连击行时用那一行，否则用基础行。对应不走这里。
static class CardRows
{
    public static CardDefinition WithCombo(FighterState fighter, CardDefinition card)
    {
        if (card.Combo != null && fighter.HasStatus(CardStatuses.Combo))
        {
            return card.Combo;
        }

        return card;
    }
}
