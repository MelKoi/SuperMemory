public readonly struct WeaponChargeTier
{
    public WeaponChargeTier(int threshold, int lifeDamage, int toughnessDamage, int range)
    {
        Threshold = threshold;
        LifeDamage = lifeDamage;
        ToughnessDamage = toughnessDamage;
        Range = range;
    }

    public int Threshold { get; }
    public int LifeDamage { get; }
    public int ToughnessDamage { get; }
    public int Range { get; }

    // charge1 / charge2：蓄能门槛、生命伤害、韧性伤害、攻击距离。
    public static WeaponChargeTier FromArray(int[] values)
    {
        if (values == null)
        {
            return default;
        }

        return new WeaponChargeTier(At(values, 0), At(values, 1), At(values, 2), At(values, 3));
    }

    public static void Resolve(int charge, WeaponChargeTier light, WeaponChargeTier heavy, out int lifeDamage, out int toughnessDamage)
    {
        lifeDamage = 0;
        toughnessDamage = 0;
        var bestThreshold = 0;
        Consider(light, ref bestThreshold, ref lifeDamage, ref toughnessDamage, charge);
        Consider(heavy, ref bestThreshold, ref lifeDamage, ref toughnessDamage, charge);
    }

    static void Consider(WeaponChargeTier tier, ref int bestThreshold, ref int lifeDamage, ref int toughnessDamage, int charge)
    {
        if (tier.Threshold <= 0 || charge < tier.Threshold || tier.Threshold < bestThreshold)
        {
            return;
        }

        bestThreshold = tier.Threshold;
        lifeDamage = tier.LifeDamage;
        toughnessDamage = tier.ToughnessDamage;
    }

    static int At(int[] values, int index)
    {
        return index < values.Length ? values[index] : 0;
    }
}
