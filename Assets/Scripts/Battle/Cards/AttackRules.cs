using System;
using System.Collections.Generic;

// 武器攻击：宣告检查，以及攻击各时点上的档位、滞留、连击、伤害和消耗。
// 连击：武器攻击 +1/+0。迅捷：本回合第一次武器攻击不消耗攻击次数。
// 攻击被打消时不造成伤害，命中后的滞留也不结算，蓄能和滞留照常消耗。
sealed class AttackRules
{
    const int ComboLifeBonus = 1;

    readonly MatchState state;
    readonly EffectResolver effects;
    int weaponSlot;
    int distance;
    string abortReason;
    bool settled;

    public AttackRules(MatchState state, EffectResolver effects)
    {
        this.state = state;
        this.effects = effects;
    }

    public bool TryAttack(BattleSide side, int slot, int targetDistance, out string error)
    {
        error = state.MainActionError(side, "当前不能攻击。");
        if (error != null)
        {
            return false;
        }

        var fighter = state.Fighter(side);
        if (slot < 0 || slot >= fighter.Weapons.Count)
        {
            error = "没有这把武器。";
            return false;
        }

        if (fighter.Weapons[slot].Charge <= 0)
        {
            error = "这把武器没有蓄能。";
            return false;
        }

        var free = fighter.HasStatus(CardStatuses.Swift) && state.Flow.WeaponAttacksThisTurn == 0;
        if (!free && state.Flow.AttacksRemaining <= 0)
        {
            error = "本回合攻击次数已经用完。";
            return false;
        }

        weaponSlot = slot;
        distance = Math.Max(0, targetDistance);
        abortReason = null;
        settled = false;
        state.Flow.BeginAttack(true, free);
        error = abortReason;
        return error == null;
    }

    public void OnTiming(AttackContext attack, AttackTiming timing)
    {
        var attacker = state.Active;
        switch (timing)
        {
            case AttackTiming.SelectLevel:
                SelectLevel(attack, attacker);
                break;
            case AttackTiming.ModifyRange:
                ApplyLinger(attacker, attack, LingerPhase.ModifyRange);
                break;
            case AttackTiming.CheckAttackCount:
                if (attack.Range < attack.Distance)
                {
                    abortReason = "攻击距离不够。";
                    state.Flow.AbortAttack();
                }

                break;
            case AttackTiming.LockAttack:
                ApplyLinger(attacker, attack, LingerPhase.LockAttack);
                if (attacker.HasStatus(CardStatuses.Combo))
                {
                    attack.LifeDamage += ComboLifeBonus;
                }

                break;
            case AttackTiming.DealDamage:
                if (attack.DealsDamage)
                {
                    DealDamage(attack, state.Inactive);
                }

                break;
            case AttackTiming.AfterHit:
                if (attack.DealsDamage)
                {
                    ApplyLinger(attacker, attack, LingerPhase.AfterHit);
                }

                break;
            case AttackTiming.Spend:
                Settle(attack, attacker);
                break;
        }
    }

    // 有人倒下时攻击走不到消耗这一步，在这里补上。
    public void SettleCurrent()
    {
        if (state.Flow.CurrentAttack != null)
        {
            Settle(state.Flow.CurrentAttack, state.Active);
        }
    }

    void SelectLevel(AttackContext attack, FighterState attacker)
    {
        var weapon = attacker.Weapons[weaponSlot];
        WeaponChargeTier.Resolve(weapon.Charge, weapon.Light, weapon.Heavy, out var life, out var toughness, out var range);
        attack.WeaponSlot = weaponSlot;
        attack.Distance = distance;
        attack.LifeDamage = life;
        attack.ToughnessDamage = toughness;
        attack.Range = range;
    }

    void ApplyLinger(FighterState attacker, AttackContext attack, LingerPhase phase)
    {
        var afterHit = new List<int[]>();
        for (var i = 0; i < attacker.Linger.Count; i++)
        {
            var step = attacker.Linger[i];
            if (LingerRules.PhaseOf(step[0]) != phase)
            {
                continue;
            }

            if (phase == LingerPhase.AfterHit)
            {
                afterHit.Add(step);
            }
            else
            {
                LingerRules.Modify(attack, step);
            }
        }

        if (afterHit.Count > 0)
        {
            effects.Resolve(attacker.Side, afterHit.ToArray());
        }
    }

    void DealDamage(AttackContext attack, FighterState defender)
    {
        if (attack.LifeDamage > 0)
        {
            state.ChangeHealth(defender, -attack.LifeDamage);
        }

        if (attack.ToughnessDamage > 0)
        {
            defender.SetToughness(defender.Toughness - attack.ToughnessDamage);
        }

        defender.TookWeaponAttack = true;
    }

    // 攻击消耗这把武器的全部蓄能，并用掉攻击方身上的全部滞留。
    void Settle(AttackContext attack, FighterState attacker)
    {
        if (settled)
        {
            return;
        }

        settled = true;
        if (attack.SpendsCharge && attack.WeaponSlot >= 0 && attack.WeaponSlot < attacker.Weapons.Count)
        {
            attacker.Weapons[attack.WeaponSlot].Charge = 0;
        }

        if (attack.ConsumesLinger)
        {
            attacker.Linger.Clear();
        }

        GainRates.Energy(attacker, MatchTimingPolicy.AttackEnergyGain);
    }
}
