// 回合时点上牌区和状态的变化。
// 加成：回合开始时敌我场上的读数都减 1。读数到 0 就结算弃置时，然后进弃牌堆。
// 临时：回合结束时还在手牌里的临时牌移出本局。去掉了子类别的不算。
// 连击：任何一方回合结束时，双方都去掉。迅捷：只在持有者自己的回合结束时去掉。
sealed class TurnCardRules
{
    readonly MatchState state;
    readonly EffectResolver effects;

    public TurnCardRules(MatchState state, EffectResolver effects)
    {
        this.state = state;
        this.effects = effects;
    }

    public void OnTiming(MatchTiming timing)
    {
        switch (timing)
        {
            case MatchTiming.TickStatuses:
                TickBuffs(state.Player);
                TickBuffs(state.Enemy);
                break;
            case MatchTiming.ExpireStatuses:
                ExpireBuffs(state.Player);
                ExpireBuffs(state.Enemy);
                break;
            case MatchTiming.RemoveTemporary:
                RemoveTemporary(state.Player);
                RemoveTemporary(state.Enemy);
                break;
            case MatchTiming.ClearRoundMarks:
                state.Active.RemoveStatus(CardStatuses.Swift);
                state.Player.RemoveStatus(CardStatuses.Combo);
                state.Enemy.RemoveStatus(CardStatuses.Combo);
                break;
        }
    }

    static void TickBuffs(FighterState fighter)
    {
        for (var i = 0; i < fighter.Field.Count; i++)
        {
            fighter.Field[i].Duration--;
        }
    }

    void ExpireBuffs(FighterState fighter)
    {
        for (var i = fighter.Field.Count - 1; i >= 0; i--)
        {
            var card = fighter.Field[i];
            if (card.Duration > 0)
            {
                continue;
            }

            fighter.Field.RemoveAt(i);
            effects.Resolve(fighter.Side, state.Cards.Get(CardKind.Buff, card.EffectCardId).EndEffect);
            fighter.Discard.Add(card);
        }
    }

    void RemoveTemporary(FighterState fighter)
    {
        for (var i = fighter.Hand.Count - 1; i >= 0; i--)
        {
            if (state.Cards.SubtypeOf(fighter.Hand[i]) != CardSubtypes.Temporary)
            {
                continue;
            }

            fighter.Removed.Add(fighter.Hand[i]);
            fighter.Hand.RemoveAt(i);
        }
    }
}
