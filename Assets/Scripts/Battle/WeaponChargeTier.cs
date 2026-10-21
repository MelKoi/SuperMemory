public readonly struct WeaponChargeTier
{
    public WeaponChargeTier(int threshold, int toughnessDamage, int lifeDamage, int range)
    {
        Threshold = threshold;
        ToughnessDamage = toughnessDamage;
        LifeDamage = lifeDamage;
        Range = range;
    }

    public int Threshold { get; }
    public int LifeDamage { get; }
    public int ToughnessDamage { get; }
    public int Range { get; }

    // charge1 / charge2：蓄能门槛、韧性伤害、生命伤害、攻击距离。效果 +x/+y 仍是 x 生命、y 韧性。
    public static WeaponChargeTier FromArray(int[] values)
    {
        if (values == null)
        {
            return default;
        }

        return new WeaponChargeTier(At(values, 0), At(values, 1), At(values, 2), At(values, 3));
    }

    // 取已达到的最高门槛。门槛相同时用后一档。
    public static void Resolve(int charge, WeaponChargeTier light, WeaponChargeTier heavy, out int lifeDamage, out int toughnessDamage)
    {
        Resolve(charge, light, heavy, out lifeDamage, out toughnessDamage, out _);
    }

    public static void Resolve(int charge, WeaponChargeTier light, WeaponChargeTier heavy, out int lifeDamage, out int toughnessDamage, out int range)
    {
        lifeDamage = 0;
        toughnessDamage = 0;
        range = 0;
        var bestThreshold = 0;
        Consider(light, ref bestThreshold, ref lifeDamage, ref toughnessDamage, ref range, charge);
        Consider(heavy, ref bestThreshold, ref lifeDamage, ref toughnessDamage, ref range, charge);
    }

    // 蓄能没达到任一档门槛时不能发动攻击。门槛为 0 的档不参与。
    public static bool CanAttack(int charge, WeaponChargeTier light, WeaponChargeTier heavy)
    {
        return Reaches(charge, light) || Reaches(charge, heavy);
    }

    static bool Reaches(int charge, WeaponChargeTier tier)
    {
        return tier.Threshold > 0 && charge >= tier.Threshold;
    }

    static void Consider(WeaponChargeTier tier, ref int bestThreshold, ref int lifeDamage, ref int toughnessDamage, ref int range, int charge)
    {
        if (tier.Threshold <= 0 || charge < tier.Threshold || tier.Threshold < bestThreshold)
        {
            return;
        }

        bestThreshold = tier.Threshold;
        lifeDamage = tier.LifeDamage;
        toughnessDamage = tier.ToughnessDamage;
        range = tier.Range;
    }

    static int At(int[] values, int index)
    {
        return index < values.Length ? values[index] : 0;
    }
}
