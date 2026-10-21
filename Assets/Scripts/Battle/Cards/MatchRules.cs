using System;
using System.Collections.Generic;
using System.IO;

// 攻击牌：加入手牌立刻发生，其余效果变成自己的滞留，下一次武器攻击结算。
// 行动牌：立刻结算，然后进入弃牌堆。对应时改走对应行，费用用对应行。
// 加成牌：放到场上。展开时立刻结算。之后每个回合开始，先结算场上的展开中效果，再把读数减 1；读数到 0 就结算弃置效果并离开。
public sealed class MatchRules : IMatchTimingHandler
{
    readonly CardCatalog cards;
    readonly FighterState player;
    readonly FighterState enemy;
    readonly List<CardInstance> addedThisPass = new List<CardInstance>();
    MatchFlow flow;
    int pendingWeaponSlot;
    int pendingDistance;
    string pendingAttackError;
    bool attackSettled;
    bool stopEffects;

    MatchRules(CardCatalog cards, FighterState player, FighterState enemy)
    {
        this.cards = cards ?? throw new ArgumentNullException(nameof(cards));
        this.player = player ?? throw new ArgumentNullException(nameof(player));
        this.enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
    }

    public CardCatalog Cards => cards;
    public MatchFlow Flow => flow;
    public FighterState Player => player;
    public FighterState Enemy => enemy;

    public static MatchRules Create(CardCatalog cards, FighterState player, FighterState enemy)
    {
        var rules = new MatchRules(cards, player, enemy);
        rules.flow = new MatchFlow(rules);
        return rules;
    }

    public FighterState GetFighter(BattleSide side)
    {
        return side == player.Side ? player : enemy;
    }

    public void Begin(BattleSide firstPlayer)
    {
        flow.Begin(firstPlayer);
    }

    public bool TryPlay(BattleSide side, int handIndex, bool asResponse, out string error)
    {
        error = null;
        var fighter = GetFighter(side);
        if (flow.IsFinished)
        {
            error = "这一局已经结束。";
            return false;
        }

        if (asResponse)
        {
            if (flow.CurrentAttack == null || !flow.CurrentAttack.WaitingForResponses)
            {
                error = "当前没有对应窗口。";
                return false;
            }

            if (side == flow.ActiveSide)
            {
                error = "不能对应自己的攻击。";
                return false;
            }
        }
        else if (flow.Timing != MatchTiming.Main || !flow.IsWaiting || flow.AttackInProgress)
        {
            error = "当前不能打出这张牌。";
            return false;
        }
        else if (side != flow.ActiveSide)
        {
            error = "现在不是这一方的主要阶段。";
            return false;
        }

        if (handIndex < 0 || handIndex >= fighter.Hand.Count)
        {
            error = "手牌不存在。";
            return false;
        }

        var instance = fighter.Hand[handIndex];
        var card = cards.Get(instance.Kind, instance.CardId);
        var owner = card.Owner;
        if (asResponse && card.Response != null && card.Response.Owner.Length > 0)
        {
            owner = card.Response.Owner;
        }

        if (!fighter.Owns(owner))
        {
            error = "这张牌不属于当前角色或武器。";
            return false;
        }

        if (card.Subtype == CardSubtypes.Restricted)
        {
            error = "限制牌不能打出，只能用于蓄能。";
            return false;
        }

        if (asResponse)
        {
            if (card.Subtype != CardSubtypes.Response || card.Response == null)
            {
                error = "这张牌不能用于对应。";
                return false;
            }
        }
        else if (card.Subtype == CardSubtypes.AllOut && flow.MainActionCount != 0)
        {
            error = "全力必须是主要阶段的第一个行动。";
            return false;
        }

        var row = SelectRow(fighter, card, asResponse);
        stopEffects = false;
        var cost = (asResponse ? card.Response.Cost : card.Cost) + fighter.NextCostDelta;
        if (cost < 0)
        {
            cost = 0;
        }

        if (fighter.Energy < cost)
        {
            error = "能量不足。";
            return false;
        }

        fighter.NextCostDelta = 0;
        fighter.Energy -= cost;
        fighter.Hand.RemoveAt(handIndex);
        var comboAlready = ComboAlready(fighter, asResponse);
        if (card.Kind == CardKind.Buff && !asResponse)
        {
            instance.EffectCardId = row.Id;
            instance.Duration = card.Duration;
            fighter.Field.Add(instance);
            flow.CompleteCardPlay(card.Subtype == CardSubtypes.AllOut, card.Combo != null, false, comboAlready);
            Resolve(fighter.Side, row.StartEffect);
        }
        else
        {
            fighter.Discard.Add(instance);
            if (asResponse)
            {
                flow.NoteResponsePlayed();
            }
            else
            {
                flow.CompleteCardPlay(card.Subtype == CardSubtypes.AllOut, card.Combo != null, false, comboAlready);
            }

            if (card.Kind == CardKind.Attack && !asResponse)
            {
                StoreAttackPlay(fighter.Side, row.PlayEffect);
            }
            else
            {
                Resolve(fighter.Side, EffectsOf(card.Kind, row));
            }
        }

        if (!flow.IsFinished && !asResponse && card.Subtype == CardSubtypes.AllOut)
        {
            flow.EndMainPhase(MainEndReason.AllOut);
        }

        return true;
    }

    public bool TryAttack(BattleSide side, int weaponSlot, int distance, out string error)
    {
        error = null;
        pendingAttackError = null;
        attackSettled = false;
        if (flow.IsFinished || flow.Timing != MatchTiming.Main || !flow.IsWaiting || flow.AttackInProgress)
        {
            error = "当前不能攻击。";
            return false;
        }

        if (side != flow.ActiveSide)
        {
            error = "现在不是这一方的主要阶段。";
            return false;
        }

        var fighter = GetFighter(side);
        if (weaponSlot < 0 || weaponSlot >= fighter.Weapons.Count)
        {
            error = "没有这把武器。";
            return false;
        }

        if (fighter.Weapons[weaponSlot].Charge <= 0)
        {
            error = "这把武器没有蓄能。";
            return false;
        }

        var freeSwift = fighter.HasStatus(CardStatuses.Swift) && flow.WeaponAttacksThisTurn == 0;
        if (!freeSwift && flow.AttacksRemaining <= 0)
        {
            error = "本回合攻击次数已经用完。";
            return false;
        }

        pendingWeaponSlot = weaponSlot;
        pendingDistance = distance < 0 ? 0 : distance;
        if (fighter.HasStatus(CardStatuses.Swift))
        {
            flow.SetSwift(true);
        }

        flow.BeginAttack(true);
        if (pendingAttackError != null)
        {
            error = pendingAttackError;
            return false;
        }

        return true;
    }

    public void CloseResponseWindow()
    {
        flow.CloseResponseWindow();
    }

    public void EndMainPhase()
    {
        flow.EndMainPhase(flow.PendingAllOut ? MainEndReason.AllOut : MainEndReason.Declared);
    }

    public void OnMatchTiming(MatchFlow match, MatchTiming timing)
    {
        if (timing == MatchTiming.TurnStartEffects)
        {
            ResolveFieldOngoing(GetFighter(match.ActiveSide));
            ResolveFieldOngoing(GetFighter(MatchFlow.Opposite(match.ActiveSide)));
        }
        else if (timing == MatchTiming.TickStatuses)
        {
            Tick(player);
            Tick(enemy);
        }
        else if (timing == MatchTiming.ExpireStatuses)
        {
            Expire(player);
            Expire(enemy);
        }
        else if (timing == MatchTiming.RemoveTemporary)
        {
            RemoveTemporary(player);
            RemoveTemporary(enemy);
        }
        else if (timing == MatchTiming.ClearRoundMarks)
        {
            GetFighter(match.ActiveSide).RemoveStatus(CardStatuses.Swift);
        }
    }

    public void OnAttackTiming(MatchFlow match, AttackContext attack, AttackTiming timing)
    {
        var attacker = GetFighter(match.ActiveSide);
        if (timing == AttackTiming.SelectLevel)
        {
            var weapon = attacker.Weapons[pendingWeaponSlot];
            attack.WeaponSlot = pendingWeaponSlot;
            attack.Distance = pendingDistance;
            WeaponChargeTier.Resolve(weapon.Charge, weapon.Light, weapon.Heavy, out var life, out var toughness, out var range);
            attack.LifeDamage = life;
            attack.ToughnessDamage = toughness;
            attack.Range = range;
        }
        else if (timing == AttackTiming.ModifyRange)
        {
            ApplyLinger(attacker, attack, LingerPhase.Range);
        }
        else if (timing == AttackTiming.CheckAttackCount)
        {
            if (attack.Range < attack.Distance)
            {
                pendingAttackError = "攻击距离不够。";
                match.AbortAttack();
            }
        }
        else if (timing == AttackTiming.LockAttack)
        {
            ApplyLinger(attacker, attack, LingerPhase.Damage);
            if (attacker.HasStatus(CardStatuses.Combo) || attack.ComboAtDeclaration)
            {
                attack.LifeDamage += 1;
            }
        }
        else if (timing == AttackTiming.DealDamage)
        {
            if (!attack.DealsDamage)
            {
                return;
            }

            var defender = GetFighter(MatchFlow.Opposite(match.ActiveSide));
            if (attack.LifeDamage > 0)
            {
                ChangeHealth(defender, -attack.LifeDamage);
            }

            if (attack.ToughnessDamage > 0)
            {
                defender.SetToughness(defender.Toughness - attack.ToughnessDamage);
            }
        }
        else if (timing == AttackTiming.AfterHit)
        {
            if (attack.DealsDamage)
            {
                ApplyLinger(attacker, attack, LingerPhase.AfterHit);
            }
        }
        else if (timing == AttackTiming.Spend)
        {
            SettleAttack(attack, attacker);
        }
    }

    CardDefinition SelectRow(FighterState fighter, CardDefinition card, bool asResponse)
    {
        if (asResponse)
        {
            return card.Response;
        }

        if (ComboAlready(fighter, false) && card.Combo != null)
        {
            return card.Combo;
        }

        return card;
    }

    bool ComboAlready(FighterState fighter, bool asResponse)
    {
        if (fighter.HasStatus(CardStatuses.Combo))
        {
            return true;
        }

        return !asResponse && flow.ActiveSide == fighter.Side && flow.ComboActive;
    }

    static int[][] EffectsOf(CardKind kind, CardDefinition row)
    {
        if (kind == CardKind.Buff)
        {
            return row.StartEffect;
        }

        return row.PlayEffect;
    }

    void StoreAttackPlay(BattleSide source, int[][] effects)
    {
        if (effects == null)
        {
            return;
        }

        for (var i = 0; i < effects.Length; i++)
        {
            var step = effects[i];
            if (IsEmpty(step))
            {
                continue;
            }

            if (step[0] == CardEffects.AddCard)
            {
                ApplyStep(source, step);
            }
            else
            {
                StoreLinger(source, step);
            }
        }
    }

    void Resolve(BattleSide source, int[][] effects)
    {
        if (effects == null || stopEffects || flow.IsFinished)
        {
            return;
        }

        addedThisPass.Clear();
        for (var i = 0; i < effects.Length; i++)
        {
            if (stopEffects || flow.IsFinished)
            {
                break;
            }

            if (!IsEmpty(effects[i]))
            {
                ApplyStep(source, effects[i]);
            }
        }
    }

    void ApplyStep(BattleSide source, int[] step)
    {
        var effect = step[0];
        if (effect == CardEffects.Draw)
        {
            Draw(GetFighter(Target(source, Arg(step, 2))), Arg(step, 1));
        }
        else if (effect == CardEffects.Energy)
        {
            ChangeEnergy(GetFighter(Target(source, Arg(step, 2))), Arg(step, 1));
        }
        else if (effect == CardEffects.Toughness)
        {
            var fighter = GetFighter(Target(source, Arg(step, 2)));
            fighter.SetToughness(fighter.Toughness + Arg(step, 1));
        }
        else if (effect == CardEffects.Life)
        {
            ChangeHealth(GetFighter(Target(source, Arg(step, 2))), Arg(step, 1));
        }
        else if (effect == CardEffects.Strike)
        {
            if (flow.AttackInProgress)
            {
                flow.CurrentAttack.LifeDamage += Arg(step, 1);
                flow.CurrentAttack.ToughnessDamage += Arg(step, 2);
            }
            else
            {
                StoreLinger(source, step);
            }
        }
        else if (effect == CardEffects.AddCard)
        {
            AddCards(GetFighter(Target(source, Arg(step, 4))), Arg(step, 1), Arg(step, 2), Arg(step, 3));
        }
        else if (effect == CardEffects.Range)
        {
            if (flow.AttackInProgress)
            {
                flow.CurrentAttack.Range += Arg(step, 1);
            }
            else
            {
                StoreLinger(source, step);
            }
        }
        else if (effect == CardEffects.Cost)
        {
            GetFighter(Target(source, Arg(step, 2))).NextCostDelta += Arg(step, 1);
        }
        else if (effect == CardEffects.CannotRespond)
        {
            if (flow.AttackInProgress)
            {
                flow.CurrentAttack.BlockResponses = true;
            }
            else
            {
                StoreLinger(source, step);
            }
        }
        else if (effect == CardEffects.Discard)
        {
            DiscardCards(GetFighter(Target(source, Arg(step, 2))), Arg(step, 1), false);
        }
        else if (effect == CardEffects.Status)
        {
            var fighter = GetFighter(Target(source, Arg(step, 2)));
            fighter.AddStatus(Arg(step, 1));
            if (Arg(step, 1) == CardStatuses.Swift && flow.ActiveSide == fighter.Side)
            {
                flow.SetSwift(true);
            }
        }
        else if (effect == CardEffects.Negate)
        {
            if (flow.CurrentAttack == null)
            {
                throw new InvalidDataException("没有正在进行的攻击。");
            }

            flow.CurrentAttack.Negated = true;
        }
        else if (effect == CardEffects.RemoveSubtype)
        {
            RemoveSubtype(GetFighter(Target(source, Arg(step, 2))), Arg(step, 1));
        }
        else if (effect == CardEffects.DiscardOrNegate)
        {
            if (flow.CurrentAttack == null)
            {
                throw new InvalidDataException("没有正在进行的攻击。");
            }

            var target = GetFighter(Target(source, Arg(step, 2)));
            if (!DiscardCards(target, Arg(step, 1), true))
            {
                flow.CurrentAttack.Negated = true;
            }
        }
        else if (effect != CardEffects.None)
        {
            throw new InvalidDataException("未知效果：" + effect);
        }
    }

    void StoreLinger(BattleSide source, int[] step)
    {
        var bucket = source;
        var effect = step[0];
        if (effect == CardEffects.Energy || effect == CardEffects.Strike || effect == CardEffects.Range || effect == CardEffects.CannotRespond)
        {
            bucket = Target(source, step[step.Length - 1]);
        }

        GetFighter(bucket).Linger.Add(Copy(step));
    }

    void ApplyLinger(FighterState owner, AttackContext attack, LingerPhase phase)
    {
        for (var i = 0; i < owner.Linger.Count; i++)
        {
            var step = owner.Linger[i];
            if (IsEmpty(step))
            {
                continue;
            }

            var effect = step[0];
            if (phase == LingerPhase.Range && effect == CardEffects.Range)
            {
                attack.Range += Arg(step, 1);
            }
            else if (phase == LingerPhase.Range && effect == CardEffects.CannotRespond)
            {
                attack.BlockResponses = true;
            }
            else if (phase == LingerPhase.Damage && effect == CardEffects.Strike)
            {
                attack.LifeDamage += Arg(step, 1);
                attack.ToughnessDamage += Arg(step, 2);
            }
            else if (phase == LingerPhase.AfterHit && effect != CardEffects.Strike && effect != CardEffects.Range && effect != CardEffects.CannotRespond)
            {
                ApplyStep(owner.Side, step);
            }
        }
    }

    void AddCards(FighterState target, int table, int cardId, int count)
    {
        if (count < 0)
        {
            throw new InvalidDataException("加入手牌的张数不能是负数。");
        }

        var kind = KindOfTable(table);
        cards.Get(kind, cardId);
        for (var i = 0; i < count; i++)
        {
            var instance = new CardInstance(kind, cardId);
            target.Hand.Add(instance);
            addedThisPass.Add(instance);
        }
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

    bool DiscardCards(FighterState target, int count, bool requireAll)
    {
        if (count < 0)
        {
            throw new InvalidDataException("弃置张数不能是负数。");
        }

        if (requireAll && target.Hand.Count < count)
        {
            return false;
        }

        var times = count < target.Hand.Count ? count : target.Hand.Count;
        for (var i = 0; i < times; i++)
        {
            var last = target.Hand.Count - 1;
            var card = target.Hand[last];
            target.Hand.RemoveAt(last);
            target.Discard.Add(card);
        }

        return true;
    }

    void RemoveSubtype(FighterState target, int subtypeId)
    {
        CardInstance chosen = null;
        for (var i = addedThisPass.Count - 1; i >= 0; i--)
        {
            if (target.Hand.Contains(addedThisPass[i]) && SubtypeOf(addedThisPass[i]) == subtypeId)
            {
                chosen = addedThisPass[i];
                break;
            }
        }

        if (chosen == null)
        {
            for (var i = target.Hand.Count - 1; i >= 0; i--)
            {
                if (SubtypeOf(target.Hand[i]) == subtypeId)
                {
                    chosen = target.Hand[i];
                    break;
                }
            }
        }

        if (chosen != null)
        {
            chosen.SubtypeOverride = CardSubtypes.None;
        }
    }

    int SubtypeOf(CardInstance card)
    {
        if (card.SubtypeOverride.HasValue)
        {
            return card.SubtypeOverride.Value;
        }

        return cards.Get(card.Kind, card.CardId).Subtype;
    }

    void ChangeHealth(FighterState fighter, int delta)
    {
        var before = fighter.Health;
        fighter.SetHealth(before + delta);
        if (before > 0 && fighter.Health == 0)
        {
            KnockOut(fighter.Side);
        }
    }

    void ChangeEnergy(FighterState fighter, int delta)
    {
        var value = fighter.Energy + delta;
        fighter.Energy = value < 0 ? 0 : value;
    }

    void KnockOut(BattleSide dead)
    {
        if (flow.AttackInProgress && flow.CurrentAttack != null)
        {
            SettleAttack(flow.CurrentAttack, GetFighter(flow.ActiveSide));
        }

        stopEffects = true;
        if (!flow.IsFinished)
        {
            flow.Finish(MatchFlow.Opposite(dead));
        }
    }

    void SettleAttack(AttackContext attack, FighterState attacker)
    {
        if (attackSettled)
        {
            return;
        }

        attackSettled = true;
        if (attack.SpendsCharge && attack.WeaponSlot >= 0 && attack.WeaponSlot < attacker.Weapons.Count)
        {
            attacker.Weapons[attack.WeaponSlot].Charge = 0;
        }

        if (attack.ConsumesLinger)
        {
            attacker.Linger.Clear();
        }
    }

    void Tick(FighterState fighter)
    {
        for (var i = 0; i < fighter.Field.Count; i++)
        {
            fighter.Field[i].Duration--;
        }
    }

    void Expire(FighterState fighter)
    {
        for (var i = fighter.Field.Count - 1; i >= 0; i--)
        {
            var card = fighter.Field[i];
            if (card.Duration > 0)
            {
                continue;
            }

            fighter.Field.RemoveAt(i);
            var row = cards.Get(CardKind.Buff, card.EffectCardId);
            Resolve(fighter.Side, row.EndEffect);
            fighter.Discard.Add(card);
        }
    }

    void ResolveFieldOngoing(FighterState fighter)
    {
        for (var i = 0; i < fighter.Field.Count; i++)
        {
            var row = cards.Get(CardKind.Buff, fighter.Field[i].EffectCardId);
            Resolve(fighter.Side, row.OngoingEffect);
        }
    }

    void RemoveTemporary(FighterState fighter)
    {
        for (var i = fighter.Hand.Count - 1; i >= 0; i--)
        {
            if (SubtypeOf(fighter.Hand[i]) != CardSubtypes.Temporary)
            {
                continue;
            }

            fighter.Removed.Add(fighter.Hand[i]);
            fighter.Hand.RemoveAt(i);
        }
    }

    static CardKind KindOfTable(int table)
    {
        if (table == (int)CardKind.Attack)
        {
            return CardKind.Attack;
        }

        if (table == (int)CardKind.Action)
        {
            return CardKind.Action;
        }

        if (table == (int)CardKind.Buff)
        {
            return CardKind.Buff;
        }

        throw new InvalidDataException("未知卡牌表号：" + table);
    }

    static BattleSide Target(BattleSide source, int target)
    {
        if (target == 0)
        {
            return source;
        }

        if (target == 1)
        {
            return MatchFlow.Opposite(source);
        }

        throw new InvalidDataException("目标只能是 0 或 1。");
    }

    static int Arg(int[] step, int index)
    {
        if (step == null || index < 0 || index >= step.Length)
        {
            throw new InvalidDataException("效果参数不足。");
        }

        return step[index];
    }

    static bool IsEmpty(int[] step)
    {
        return step == null || step.Length == 0 || step[0] == CardEffects.None;
    }

    static int[] Copy(int[] step)
    {
        var copy = new int[step.Length];
        Array.Copy(step, copy, step.Length);
        return copy;
    }

    enum LingerPhase
    {
        Range,
        Damage,
        AfterHit,
    }
}
