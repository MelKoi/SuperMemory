using System.IO;

// 滞留：攻击牌的效果等到下一次武器攻击再结算。
// 打击、射程、不能对应修正攻击本身，挂在目标身上，修正目标的下一次攻击。
// 其余效果挂在打出者身上，命中后按打出者的视角结算。
static class LingerRules
{
    public static void Store(MatchState state, BattleSide source, int[] step)
    {
        var owner = CardEffects.ModifiesAttack(step[0]) ? state.Target(source, step) : state.Fighter(source);
        owner.Linger.Add(step);
    }

    public static LingerPhase PhaseOf(int effect)
    {
        switch (effect)
        {
            case CardEffects.Range:
            case CardEffects.CannotRespond:
                return LingerPhase.ModifyRange;
            case CardEffects.Strike:
                return LingerPhase.LockAttack;
            default:
                return LingerPhase.AfterHit;
        }
    }

    public static void Modify(AttackContext attack, int[] step)
    {
        switch (step[0])
        {
            case CardEffects.Strike:
                attack.LifeDamage += step[1];
                attack.ToughnessDamage += step[2];
                break;
            case CardEffects.Range:
                attack.Range += step[1];
                break;
            case CardEffects.CannotRespond:
                attack.BlockResponses = true;
                break;
            default:
                throw new InvalidDataException("效果 " + step[0] + " 不修正攻击。");
        }
    }
}

enum LingerPhase
{
    ModifyRange,
    LockAttack,
    AfterHit,
}
