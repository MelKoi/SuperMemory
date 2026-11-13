using System;

// 破绽：韧性为 0 时，正数的能量和蓄能按比例向下取整。扣能量不打折。
public static class GainRates
{
    public static int Scale(FighterState fighter, int amount)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (amount <= 0 || fighter.Toughness > 0 || MatchTimingPolicy.VulnerabilityDenominator <= 0)
        {
            return amount;
        }

        return amount * MatchTimingPolicy.VulnerabilityNumerator / MatchTimingPolicy.VulnerabilityDenominator;
    }

    public static void Energy(FighterState fighter, int amount)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        fighter.Energy = Math.Max(0, fighter.Energy + Scale(fighter, amount));
    }
}
