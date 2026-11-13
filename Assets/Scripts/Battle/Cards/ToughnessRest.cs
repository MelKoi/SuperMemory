using System;

// 回合开始的韧性回复。上一回合受到过武器攻击的一方这次不回复，标记随后清掉。
public static class ToughnessRest
{
    public static void Apply(FighterState fighter)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (fighter.TookWeaponAttack)
        {
            fighter.TookWeaponAttack = false;
            return;
        }

        fighter.SetToughness(fighter.Toughness + MatchTimingPolicy.ToughnessRecovery);
    }
}
