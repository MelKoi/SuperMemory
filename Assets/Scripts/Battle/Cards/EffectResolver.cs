using System;
using System.Collections.Generic;
using System.IO;

// 卡牌效果按顺序排队结算。弃置要目标一方选牌，选好之前整条队停住，其他操作也不能进行。
// 队从空开始到再次清空算一轮。去掉子类别优先找这一轮刚加入手牌的牌。
sealed class EffectResolver
{
    readonly MatchState state;
    readonly List<QueuedStep> queue = new List<QueuedStep>();
    readonly List<CardInstance> addedThisPass = new List<CardInstance>();
    Action whenIdle;

    public EffectResolver(MatchState state)
    {
        this.state = state;
    }

    public bool WaitingForDiscard { get; private set; }
    public BattleSide DiscardSide { get; private set; }
    public int DiscardCount { get; private set; }
    public string DiscardPrompt => "请先选择要弃置的 " + DiscardCount + " 张手牌。";
    bool IsIdle => queue.Count == 0 && !WaitingForDiscard;

    public void Resolve(BattleSide source, int[][] effects)
    {
        if (state.Flow.IsFinished)
        {
            return;
        }

        if (IsIdle)
        {
            addedThisPass.Clear();
        }

        for (var i = 0; i < effects.Length; i++)
        {
            if (!CardEffects.IsEmpty(effects[i]))
            {
                queue.Add(new QueuedStep(source, effects[i]));
            }
        }

        Pump();
    }

    // 队已经空了就马上执行，否则等这一轮结算完、弃牌也选完再执行。
    public void WhenIdle(Action action)
    {
        if (IsIdle)
        {
            action();
        }
        else
        {
            whenIdle = action;
        }
    }

    // 目标一方从自己的手牌里选出正好 DiscardCount 张。
    public bool TryChooseDiscard(BattleSide side, IReadOnlyList<int> handIndices, out string error)
    {
        error = null;
        if (!WaitingForDiscard)
        {
            error = "现在不需要弃牌。";
            return false;
        }

        if (side != DiscardSide)
        {
            error = "现在应由另一方选择弃牌。";
            return false;
        }

        if (handIndices == null || handIndices.Count != DiscardCount)
        {
            error = "需要选择 " + DiscardCount + " 张手牌。";
            return false;
        }

        var fighter = state.Fighter(side);
        var chosen = new HashSet<int>();
        var discarded = new CardInstance[handIndices.Count];
        for (var i = 0; i < handIndices.Count; i++)
        {
            var index = handIndices[i];
            if (index < 0 || index >= fighter.Hand.Count || !chosen.Add(index))
            {
                error = "选择的手牌不存在或重复。";
                return false;
            }

            discarded[i] = fighter.Hand[index];
        }

        for (var i = 0; i < discarded.Length; i++)
        {
            fighter.Hand.Remove(discarded[i]);
            fighter.Discard.Add(discarded[i]);
        }

        WaitingForDiscard = false;
        DiscardCount = 0;
        Pump();
        return true;
    }

    void Pump()
    {
        while (queue.Count > 0 && !WaitingForDiscard)
        {
            if (state.Flow.IsFinished)
            {
                queue.Clear();
                break;
            }

            var next = queue[0];
            queue.RemoveAt(0);
            Apply(next.Source, next.Step);
        }

        if (IsIdle && whenIdle != null)
        {
            var action = whenIdle;
            whenIdle = null;
            action();
        }
    }

    void Apply(BattleSide source, int[] step)
    {
        var target = state.Target(source, step);
        switch (step[0])
        {
            case CardEffects.Draw:
                Draw(target, step[1]);
                break;
            case CardEffects.Energy:
                target.Energy = Math.Max(0, target.Energy + step[1]);
                break;
            case CardEffects.Toughness:
                target.SetToughness(target.Toughness + step[1]);
                break;
            case CardEffects.Life:
                state.ChangeHealth(target, step[1]);
                break;
            case CardEffects.Strike:
            case CardEffects.Range:
            case CardEffects.CannotRespond:
                ModifyAttack(source, target, step);
                break;
            case CardEffects.AddCard:
                AddCards(target, CardKinds.FromTable(step[1]), step[2], step[3]);
                break;
            case CardEffects.Cost:
                target.NextCostDelta += step[1];
                break;
            case CardEffects.Discard:
                BeginDiscard(target, step[1], false);
                break;
            case CardEffects.Status:
                target.AddStatus(step[1]);
                break;
            case CardEffects.Negate:
                CurrentAttack().Negated = true;
                break;
            case CardEffects.RemoveSubtype:
                RemoveSubtype(target, step[1]);
                break;
            case CardEffects.DiscardOrNegate:
                BeginDiscard(target, step[1], true);
                break;
            default:
                throw new InvalidDataException("未知效果：" + step[0]);
        }
    }

    // 正在进行的攻击属于主动方。目标是主动方就改这次攻击，否则留给目标的下一次攻击。
    void ModifyAttack(BattleSide source, FighterState target, int[] step)
    {
        var flow = state.Flow;
        if (flow.AttackInProgress && target.Side == flow.ActiveSide)
        {
            LingerRules.Modify(flow.CurrentAttack, step);
        }
        else
        {
            LingerRules.Store(state, source, step);
        }
    }

    // 弃置：手牌不够就全弃。弃置或打消：手牌不够就打消这次攻击。需要选牌时队停住。
    void BeginDiscard(FighterState target, int count, bool negateIfShort)
    {
        if (count < 0)
        {
            throw new InvalidDataException("弃置张数不能是负数。");
        }

        if (negateIfShort)
        {
            var attack = CurrentAttack();
            if (target.Hand.Count < count)
            {
                attack.Negated = true;
                return;
            }
        }

        count = Math.Min(count, target.Hand.Count);
        if (count == 0)
        {
            return;
        }

        WaitingForDiscard = true;
        DiscardSide = target.Side;
        DiscardCount = count;
    }

    AttackContext CurrentAttack()
    {
        return state.Flow.CurrentAttack ?? throw new InvalidDataException("没有正在进行的攻击。");
    }

    void Draw(FighterState target, int count)
    {
        for (var i = 0; i < count && target.Deck.Count > 0; i++)
        {
            var card = target.Deck[0];
            target.Deck.RemoveAt(0);
            target.Hand.Add(card);
            addedThisPass.Add(card);
        }
    }

    void AddCards(FighterState target, CardKind kind, int cardId, int count)
    {
        if (count < 0)
        {
            throw new InvalidDataException("加入手牌的张数不能是负数。");
        }

        state.Cards.Get(kind, cardId);
        for (var i = 0; i < count; i++)
        {
            var card = new CardInstance(kind, cardId);
            target.Hand.Add(card);
            addedThisPass.Add(card);
        }
    }

    void RemoveSubtype(FighterState target, int subtype)
    {
        var chosen = LastMatching(addedThisPass, card => target.Hand.Contains(card) && state.Cards.SubtypeOf(card) == subtype)
            ?? LastMatching(target.Hand, card => state.Cards.SubtypeOf(card) == subtype);
        if (chosen != null)
        {
            chosen.SubtypeOverride = CardSubtypes.None;
        }
    }

    static CardInstance LastMatching(List<CardInstance> cards, Predicate<CardInstance> match)
    {
        for (var i = cards.Count - 1; i >= 0; i--)
        {
            if (match(cards[i]))
            {
                return cards[i];
            }
        }

        return null;
    }

    readonly struct QueuedStep
    {
        public QueuedStep(BattleSide source, int[] step)
        {
            Source = source;
            Step = step;
        }

        public BattleSide Source { get; }
        public int[] Step { get; }
    }
}
