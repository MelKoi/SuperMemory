using System;
using System.Collections.Generic;

// 卡牌规则的入口。MatchFlow 只排时点，这里接住每个时点和每个操作，交给对应的部分结算：
// CardPlayRules 打出和蓄能，AttackRules 武器攻击，EffectResolver 效果和弃牌选择，
// LingerRules 滞留，TurnCardRules 回合时点上的牌区和状态。
// 等待弃牌选择时，其他操作都不能进行。
public sealed class MatchRules : IMatchTimingHandler
{
    readonly MatchState state;
    readonly EffectResolver effects;
    readonly CardPlayRules plays;
    readonly AttackRules attacks;
    readonly TurnCardRules turns;

    MatchRules(CardCatalog cards, FighterState player, FighterState enemy)
    {
        state = new MatchState(cards, player, enemy, this);
        effects = new EffectResolver(state);
        plays = new CardPlayRules(state, effects);
        attacks = new AttackRules(state, effects);
        turns = new TurnCardRules(state, effects);
        state.KnockedOut = KnockOut;
    }

    public CardCatalog Cards => state.Cards;
    public MatchFlow Flow => state.Flow;
    public FighterState Player => state.Player;
    public FighterState Enemy => state.Enemy;
    public bool WaitingForDiscard => effects.WaitingForDiscard;
    public BattleSide DiscardSide => effects.DiscardSide;
    public int DiscardCount => effects.DiscardCount;

    public static MatchRules Create(CardCatalog cards, FighterState player, FighterState enemy)
    {
        return new MatchRules(cards, player, enemy);
    }

    public FighterState GetFighter(BattleSide side)
    {
        return state.Fighter(side);
    }

    public void Begin(BattleSide firstPlayer)
    {
        state.Flow.Begin(firstPlayer);
    }

    public bool TryPlay(BattleSide side, int handIndex, bool asResponse, out string error)
    {
        return !IsBlocked(out error) && plays.TryPlay(side, handIndex, asResponse, out error);
    }

    public bool TryChooseDiscard(BattleSide side, IReadOnlyList<int> handIndices, out string error)
    {
        return effects.TryChooseDiscard(side, handIndices, out error);
    }

    public bool TryCharge(BattleSide side, int handIndex, int weaponSlot, out string error)
    {
        return !IsBlocked(out error) && plays.TryCharge(side, handIndex, weaponSlot, out error);
    }

    public bool TryAttack(BattleSide side, int weaponSlot, int distance, out string error)
    {
        return !IsBlocked(out error) && attacks.TryAttack(side, weaponSlot, distance, out error);
    }

    public void CloseResponseWindow()
    {
        EnsureNoDiscard();
        state.Flow.CloseResponseWindow();
    }

    public void EndMainPhase()
    {
        EnsureNoDiscard();
        state.Flow.EndMainPhase(state.Flow.PendingAllOut ? MainEndReason.AllOut : MainEndReason.Declared);
    }

    public void OnMatchTiming(MatchFlow match, MatchTiming timing)
    {
        turns.OnTiming(timing);
    }

    public void OnAttackTiming(MatchFlow match, AttackContext attack, AttackTiming timing)
    {
        attacks.OnTiming(attack, timing);
    }

    bool IsBlocked(out string error)
    {
        error = null;
        if (state.Flow.IsFinished)
        {
            error = "这一局已经结束。";
        }
        else if (effects.WaitingForDiscard)
        {
            error = effects.DiscardPrompt;
        }

        return error != null;
    }

    void EnsureNoDiscard()
    {
        if (effects.WaitingForDiscard)
        {
            throw new InvalidOperationException(effects.DiscardPrompt);
        }
    }

    void KnockOut(FighterState dead)
    {
        attacks.SettleCurrent();
        state.Flow.Finish(MatchFlow.Opposite(dead.Side));
    }
}
