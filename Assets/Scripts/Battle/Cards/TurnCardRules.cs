// 流程真正走到的回合时点。只改牌区和状态。
// 读数：敌我场上的加成都减 1，到 0 就结算弃置时，然后进弃牌堆。
// 展开中：主要阶段结束后，结算当前这一方场上加成牌自己的展开中效果。已有连击且有连击行，就用那一行。
// 临时：还在手牌里的临时牌移出本局。去掉了子类别的不算。
// 连击在任何一方回合结束时从双方去掉。迅捷只在持有者自己的回合结束时去掉。
// 回合开始效果目前没有要结算的牌。洗牌和回合能量不在这里，见 TurnShuffle。
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
            case MatchTiming.TurnStartEffects:
            case MatchTiming.TurnEnergy:
                break;
            case MatchTiming.ToughnessRecovery:
                ToughnessRest.Apply(state.Active);
                break;
            case MatchTiming.TickStatuses:
                BuffDurations.TickBoth(state.Player, state.Enemy);
                break;
            case MatchTiming.ExpireStatuses:
                BuffDurations.ExpireBoth(state.Player, state.Enemy, state.Cards, ResolveEndEffect);
                break;
            case MatchTiming.OngoingEffects:
                ResolveOngoing(state.Active);
                break;
            case MatchTiming.RemoveTemporary:
                RemoveTemporary(state.Player);
                RemoveTemporary(state.Enemy);
                break;
            case MatchTiming.AdjustHand:
                AdjustHand(state.Active);
                break;
            case MatchTiming.ClearRoundMarks:
                state.Active.RemoveStatus(CardStatuses.Swift);
                state.Player.RemoveStatus(CardStatuses.Combo);
                state.Enemy.RemoveStatus(CardStatuses.Combo);
                break;
        }
    }

    void ResolveEndEffect(FighterState fighter, int[][] endEffect)
    {
        effects.Resolve(fighter.Side, endEffect);
    }

    void ResolveOngoing(FighterState fighter)
    {
        for (var i = 0; i < fighter.Field.Count; i++)
        {
            var instance = fighter.Field[i];
            var card = CardRows.WithCombo(fighter, state.Cards.Get(instance.Kind, instance.CardId));
            effects.Resolve(fighter.Side, card.OngoingEffect);
        }
    }

    static void AdjustHand(FighterState fighter)
    {
        while (fighter.Hand.Count < fighter.HandLimit)
        {
            if (DeckOrder.DrawOne(fighter) == null)
            {
                break;
            }
        }

        while (fighter.Hand.Count > fighter.HandLimit)
        {
            var last = fighter.Hand.Count - 1;
            var card = fighter.Hand[last];
            fighter.Hand.RemoveAt(last);
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
