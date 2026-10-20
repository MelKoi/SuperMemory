using System;
using System.Collections.Generic;

// 局前 → 开局 → 回合开始 → 主要阶段 → 主要阶段结束 → 回合结束。
// 洗牌和主要阶段会停住。武器攻击在主要阶段里另走一条时点，对应窗口也会停住。
// 这里只排时点，不结算牌面。
public sealed class MatchFlow
{
    private readonly IMatchTimingHandler handler;
    private readonly List<MatchTiming> trace = new List<MatchTiming>();
    private readonly List<MainActionRecord> actions = new List<MainActionRecord>();

    private bool started;
    private bool finished;
    private bool waiting;
    private int segmentIndex;
    private Segment segment;

    private enum Segment
    {
        BeforeMatch,
        Opening,
        Turn,
    }

    public MatchFlow()
        : this(NullMatchTimingHandler.Instance)
    {
    }

    public MatchFlow(IMatchTimingHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        this.handler = handler;
    }

    public MatchTiming Timing { get; private set; }
    public bool IsWaiting => waiting;
    public bool IsFinished => finished;
    public BattleSide FirstPlayer { get; private set; }
    public BattleSide ActiveSide { get; private set; }
    public int TurnIndex { get; private set; }
    public int MainActionCount { get; private set; }
    public int TraitActionsThisTurn { get; private set; }
    public bool PendingAllOut { get; private set; }
    public bool LastActionHadCombo { get; private set; }
    public bool HasMainEndReason { get; private set; }
    public MainEndReason MainEndReason { get; private set; }
    public int ShuffleCount { get; private set; }
    public bool LastShuffleDidShuffle { get; private set; }
    public AttackContext CurrentAttack { get; private set; }
    public AttackContext LastAttack { get; private set; }
    public bool HasWinner { get; private set; }
    public BattleSide Winner { get; private set; }

    public bool ComboActive => TraitActionsThisTurn >= 2;
    public bool IsFirstMainAction => MainActionCount == 0;
    public bool AttackInProgress => CurrentAttack != null;

    public IReadOnlyList<MatchTiming> Trace => trace;
    public IReadOnlyList<MainActionRecord> Actions => actions;

    public static BattleSide Opposite(BattleSide side)
    {
        return side == BattleSide.Player ? BattleSide.Enemy : BattleSide.Player;
    }

    public void Begin(BattleSide firstPlayer)
    {
        if (started)
        {
            throw new InvalidOperationException("这一局已经开始。");
        }

        started = true;
        FirstPlayer = firstPlayer;
        ActiveSide = firstPlayer;
        segment = Segment.BeforeMatch;
        segmentIndex = 0;
        EnterCurrent();
    }

    public void ResolveShuffle(bool shuffled)
    {
        if (finished || Timing != MatchTiming.Shuffle || !waiting)
        {
            throw new InvalidOperationException("当前不是洗牌时点。");
        }

        ShuffleCount++;
        LastShuffleDidShuffle = shuffled;
        waiting = false;
        Advance();
    }

    public void EndMainPhase(MainEndReason reason)
    {
        if (finished || Timing != MatchTiming.Main || !waiting || CurrentAttack != null)
        {
            throw new InvalidOperationException("当前不能结束主要阶段。");
        }

        if (reason == MainEndReason.AllOut && !PendingAllOut)
        {
            throw new InvalidOperationException("没有打出全力，不能按全力结束主要阶段。");
        }

        if (reason != MainEndReason.AllOut && PendingAllOut)
        {
            throw new InvalidOperationException("全力打出后，主要阶段按全力结束。");
        }

        MainEndReason = reason;
        HasMainEndReason = true;
        PendingAllOut = false;
        waiting = false;
        Advance();
    }

    public void CompleteCardPlay()
    {
        CompleteCardPlay(false);
    }

    public void CompleteCardPlay(bool allOut)
    {
        EnsureIdleMain();
        if (allOut && MainActionCount != 0)
        {
            throw new InvalidOperationException("全力必须是主要阶段的第一个行动。");
        }

        LastActionHadCombo = ComboActive;
        MainActionCount++;
        TraitActionsThisTurn++;
        PendingAllOut = allOut;
        RecordAction(MainActionKind.PlayCard, LastActionHadCombo);
    }

    public void CompleteCharge()
    {
        EnsureIdleMain();
        LastActionHadCombo = ComboActive;
        MainActionCount++;
        RecordAction(MainActionKind.Charge, LastActionHadCombo);
    }

    public void CompleteMove()
    {
        EnsureIdleMain();
        LastActionHadCombo = ComboActive;
        MainActionCount++;
        RecordAction(MainActionKind.Move, LastActionHadCombo);
    }

    public void BeginAttack(bool canRespond)
    {
        EnsureIdleMain();
        CurrentAttack = new AttackContext(canRespond, ComboActive);
        EnterAttackStep();
    }

    public void CloseResponseWindow()
    {
        if (CurrentAttack == null || !CurrentAttack.WaitingForResponses)
        {
            throw new InvalidOperationException("当前没有对应窗口。");
        }

        CurrentAttack.SetWaitingForResponses(false);
        ContinueAttack();
    }

    public void NoteResponsePlayed()
    {
        if (CurrentAttack == null)
        {
            throw new InvalidOperationException("当前没有攻击。");
        }

        if (CurrentAttack.Timing != AttackTiming.ResponseWindow && CurrentAttack.Timing != AttackTiming.ResolveResponses)
        {
            throw new InvalidOperationException("对应只能在对应窗口或对应结算时打出。");
        }

        CurrentAttack.AddResponse();
    }

    public void AbortAttack()
    {
        if (CurrentAttack == null || CurrentAttack.Timing != AttackTiming.CheckAttackCount)
        {
            throw new InvalidOperationException("只有在攻击次数检查时才能取消这次宣告。");
        }

        CurrentAttack.MarkAborted();
    }

    public void Finish(BattleSide winner)
    {
        if (finished)
        {
            return;
        }

        finished = true;
        HasWinner = true;
        Winner = winner;
        waiting = false;
        if (CurrentAttack != null)
        {
            CurrentAttack.SetWaitingForResponses(false);
        }

        Timing = MatchTiming.Finished;
        trace.Add(MatchTiming.Finished);
        handler.OnMatchTiming(this, MatchTiming.Finished);
    }

    private void EnterCurrent()
    {
        MatchTiming timing = CurrentTiming();
        Timing = timing;
        waiting = IsDecision(timing);
        trace.Add(timing);

        if (timing == MatchTiming.ClearRoundMarks)
        {
            TraitActionsThisTurn = 0;
        }

        if (timing == MatchTiming.Main)
        {
            MainActionCount = 0;
            PendingAllOut = false;
        }

        handler.OnMatchTiming(this, timing);
        if (finished || Timing != timing)
        {
            return;
        }

        if (!waiting)
        {
            Advance();
        }
    }

    private void Advance()
    {
        if (finished)
        {
            return;
        }

        segmentIndex++;
        if (segmentIndex < SegmentLength(segment))
        {
            EnterCurrent();
            return;
        }

        if (segment == Segment.BeforeMatch)
        {
            segment = Segment.Opening;
            segmentIndex = 0;
            EnterCurrent();
            return;
        }

        if (segment == Segment.Opening)
        {
            StartTurn(FirstPlayer);
            return;
        }

        StartTurn(Opposite(ActiveSide));
    }

    private void StartTurn(BattleSide side)
    {
        if (finished)
        {
            return;
        }

        ActiveSide = side;
        TurnIndex++;
        MainActionCount = 0;
        TraitActionsThisTurn = 0;
        PendingAllOut = false;
        HasMainEndReason = false;
        segment = Segment.Turn;
        segmentIndex = 0;
        EnterCurrent();
    }

    private void EnterAttackStep()
    {
        AttackContext attack = CurrentAttack;
        if (attack == null || finished)
        {
            return;
        }

        if (attack.Index >= MatchTimingOrder.Attack.Count)
        {
            CompleteAttack(attack);
            return;
        }

        AttackTiming step = MatchTimingOrder.Attack[attack.Index];
        if (ShouldSkip(attack, step))
        {
            attack.AddStep(step, true);
            attack.Index++;
            EnterAttackStep();
            return;
        }

        attack.AddStep(step, false);
        if (step == AttackTiming.CountTrait && attack.CountsForTrait)
        {
            TraitActionsThisTurn++;
        }

        if (step == AttackTiming.ResponseWindow)
        {
            attack.SetWaitingForResponses(true);
        }

        handler.OnAttackTiming(this, attack, step);
        if (finished || CurrentAttack != attack)
        {
            return;
        }

        if (attack.Aborted)
        {
            LastAttack = attack;
            CurrentAttack = null;
            return;
        }

        if (attack.WaitingForResponses)
        {
            return;
        }

        ContinueAttack();
    }

    private void ContinueAttack()
    {
        if (CurrentAttack == null || finished)
        {
            return;
        }

        CurrentAttack.Index++;
        EnterAttackStep();
    }

    private void CompleteAttack(AttackContext attack)
    {
        MainActionCount++;
        LastActionHadCombo = attack.ComboAtDeclaration;
        RecordAction(MainActionKind.Attack, attack.ComboAtDeclaration);
        LastAttack = attack;
        CurrentAttack = null;
    }

    private void RecordAction(MainActionKind kind, bool comboAlreadyActive)
    {
        actions.Add(new MainActionRecord(TurnIndex, ActiveSide, kind, comboAlreadyActive, TraitActionsThisTurn));
    }

    private void EnsureIdleMain()
    {
        if (finished || Timing != MatchTiming.Main || !waiting || CurrentAttack != null)
        {
            throw new InvalidOperationException("当前不能在主要阶段行动。");
        }

        if (PendingAllOut)
        {
            throw new InvalidOperationException("全力已经打出，主要阶段只能结束。");
        }
    }

    private MatchTiming CurrentTiming()
    {
        if (segment == Segment.BeforeMatch)
        {
            return MatchTimingOrder.BeforeMatch[segmentIndex];
        }

        if (segment == Segment.Opening)
        {
            return MatchTimingOrder.Opening[segmentIndex];
        }

        return MatchTimingOrder.Turn[segmentIndex];
    }

    private static int SegmentLength(Segment segment)
    {
        if (segment == Segment.BeforeMatch)
        {
            return MatchTimingOrder.BeforeMatch.Count;
        }

        if (segment == Segment.Opening)
        {
            return MatchTimingOrder.Opening.Count;
        }

        return MatchTimingOrder.Turn.Count;
    }

    private static bool IsDecision(MatchTiming timing)
    {
        return timing == MatchTiming.Shuffle || timing == MatchTiming.Main;
    }

    private static bool ShouldSkip(AttackContext attack, AttackTiming step)
    {
        if (attack.CanRespond)
        {
            return false;
        }

        return step == AttackTiming.ResponseWindow || step == AttackTiming.ResolveResponses;
    }
}
