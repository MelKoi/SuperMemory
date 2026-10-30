using System;
using System.Collections.Generic;

// 打出和蓄能。
// 攻击牌：加入手牌立刻发生，其余效果变成滞留，下一次武器攻击结算。进弃牌堆。
// 行动牌：立刻结算，进弃牌堆。
// 加成牌：放到场上，立刻结算展开时。展开中的时点由各牌描述决定，这里不结算。
// 对应：只在对方武器攻击的对应窗口打出，读对应行和对应行的费用，按行动牌结算。
// 全力：必须是主要阶段的第一个行动，效果结算完后结束主要阶段。
// 限制：不能打出，可以弃掉给武器蓄能。
// 连击：持有连击状态时，有连击行就改用连击行，费用仍按初始卡。对应不读连击行。
sealed class CardPlayRules
{
    readonly MatchState state;
    readonly EffectResolver effects;

    public CardPlayRules(MatchState state, EffectResolver effects)
    {
        this.state = state;
        this.effects = effects;
    }

    enum PlayMode
    {
        Instant,
        Linger,
        Field,
    }

    public bool TryPlay(BattleSide side, int handIndex, bool asResponse, out string error)
    {
        error = asResponse ? ResponseWindowError(side) : state.MainActionError(side, "当前不能打出这张牌。");
        if (error != null)
        {
            return false;
        }

        var fighter = state.Fighter(side);
        if (handIndex < 0 || handIndex >= fighter.Hand.Count)
        {
            error = "手牌不存在。";
            return false;
        }

        var instance = fighter.Hand[handIndex];
        var card = state.Cards.Get(instance.Kind, instance.CardId);
        var subtype = state.Cards.SubtypeOf(instance);
        error = PlayError(fighter, card, subtype, asResponse);
        if (error != null)
        {
            return false;
        }

        var row = SelectRow(fighter, card, asResponse);
        var mode = ModeOf(card, asResponse);
        var immediate = mode == PlayMode.Field ? row.StartEffect : row.PlayEffect;
        if (mode != PlayMode.Linger && state.Flow.CurrentAttack == null && CardEffects.AnyNeedsAttack(immediate))
        {
            error = "这张牌只能在攻击进行时使用。";
            return false;
        }

        var cost = Math.Max(0, (asResponse ? card.Response.Cost : card.Cost) + fighter.NextCostDelta);
        if (fighter.Energy < cost)
        {
            error = "能量不足。";
            return false;
        }

        fighter.NextCostDelta = 0;
        fighter.Energy -= cost;
        fighter.Hand.RemoveAt(handIndex);
        if (mode == PlayMode.Field)
        {
            instance.EffectCardId = row.Id;
            instance.Duration = card.Duration;
            fighter.Field.Add(instance);
        }
        else
        {
            fighter.Discard.Add(instance);
        }

        var allOut = !asResponse && subtype == CardSubtypes.AllOut;
        if (asResponse)
        {
            state.Flow.NoteResponsePlayed();
        }
        else
        {
            state.Flow.CompleteCardPlay(allOut);
        }

        if (mode == PlayMode.Linger)
        {
            PlayAttack(side, row.PlayEffect);
        }
        else
        {
            effects.Resolve(side, immediate);
        }

        if (allOut)
        {
            effects.WhenIdle(EndMainForAllOut);
        }

        return true;
    }

    public bool TryCharge(BattleSide side, int handIndex, int weaponSlot, out string error)
    {
        error = state.MainActionError(side, "当前不能蓄能。");
        if (error != null)
        {
            return false;
        }

        var fighter = state.Fighter(side);
        if (handIndex < 0 || handIndex >= fighter.Hand.Count)
        {
            error = "手牌不存在。";
            return false;
        }

        if (weaponSlot < 0 || weaponSlot >= fighter.Weapons.Count)
        {
            error = "没有这把武器。";
            return false;
        }

        var instance = fighter.Hand[handIndex];
        if (state.Cards.SubtypeOf(instance) == CardSubtypes.Restricted && !MatchTimingPolicy.RestrictedCardCanCharge)
        {
            error = "限制牌不能用于蓄能。";
            return false;
        }

        fighter.Hand.RemoveAt(handIndex);
        fighter.Discard.Add(instance);
        fighter.Weapons[weaponSlot].Charge += MatchTimingPolicy.ChargeGainPerCard;
        state.Flow.CompleteCharge(weaponSlot);
        return true;
    }

    string ResponseWindowError(BattleSide side)
    {
        var attack = state.Flow.CurrentAttack;
        if (attack == null || !attack.WaitingForResponses)
        {
            return "当前没有对应窗口。";
        }

        if (side == state.Flow.ActiveSide)
        {
            return "不能对应自己的攻击。";
        }

        return null;
    }

    string PlayError(FighterState fighter, CardDefinition card, int subtype, bool asResponse)
    {
        var owner = card.Owner;
        if (asResponse && card.Response != null && card.Response.Owner.Length > 0)
        {
            owner = card.Response.Owner;
        }

        if (!fighter.Owns(owner))
        {
            return "这张牌不属于当前角色或武器。";
        }

        if (subtype == CardSubtypes.Restricted)
        {
            return "限制牌不能打出，只能用于蓄能。";
        }

        if (asResponse && (subtype != CardSubtypes.Response || card.Response == null))
        {
            return "这张牌不能用于对应。";
        }

        if (!asResponse && subtype == CardSubtypes.AllOut && !state.Flow.IsFirstMainAction)
        {
            return "全力必须是主要阶段的第一个行动。";
        }

        return null;
    }

    static CardDefinition SelectRow(FighterState fighter, CardDefinition card, bool asResponse)
    {
        if (asResponse)
        {
            return card.Response;
        }

        if (card.Combo != null && fighter.HasStatus(CardStatuses.Combo))
        {
            return card.Combo;
        }

        return card;
    }

    static PlayMode ModeOf(CardDefinition card, bool asResponse)
    {
        if (asResponse || card.Kind == CardKind.Action)
        {
            return PlayMode.Instant;
        }

        return card.Kind == CardKind.Attack ? PlayMode.Linger : PlayMode.Field;
    }

    void PlayAttack(BattleSide side, int[][] playEffect)
    {
        var now = new List<int[]>();
        for (var i = 0; i < playEffect.Length; i++)
        {
            var step = playEffect[i];
            if (CardEffects.IsEmpty(step))
            {
                continue;
            }

            if (step[0] == CardEffects.AddCard)
            {
                now.Add(step);
            }
            else
            {
                LingerRules.Store(state, side, step);
            }
        }

        effects.Resolve(side, now.ToArray());
    }

    void EndMainForAllOut()
    {
        if (!state.Flow.IsFinished)
        {
            state.Flow.EndMainPhase(MainEndReason.AllOut);
        }
    }
}
