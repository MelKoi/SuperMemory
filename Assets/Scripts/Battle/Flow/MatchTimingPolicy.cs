public static class MatchTimingPolicy
{
    public const int OpeningHandCount = 3;
    public const int FirstPlayerEnergy = 3;
    public const int SecondPlayerEnergy = 5;
    public const int TurnEnergyGain = 5;
    public const int AttackEnergyGain = 1;
    public const int ToughnessRecovery = 1;
    public const int ShuffleLifeCost = 1;
    public const int BoardColumns = 10;
    public const int BoardRows = 3;
    public const int OpeningDistance = 8;

    // 表里没有这些数。破绽时获能和蓄能按这个比例向下取整。
    public const int VulnerabilityNumerator = 1;
    public const int VulnerabilityDenominator = 2;

    // 蓄能是弃一张手牌，指定一把武器，蓄能 +1，限制牌可以这样用。
    public const bool RestrictedCardCanCharge = true;
    public const int ChargeGainPerCard = 1;

    // 表里没有攻击次数的列。每回合 1 次武器攻击，用完不能宣告。迅捷的第一次不消耗这次次数。
    public const int AttacksPerTurn = 1;

    // 攻击被打消：伤害和命中后的滞留不生效，滞留仍消耗。蓄能、攻击次数照常消耗，这次宣告仍计入特质。
    public const bool NegatedAttackDealsDamage = false;
    public const bool NegatedAttackConsumesLinger = true;
    public const bool NegatedAttackSpendsCharge = true;
    public const bool NegatedAttackSpendsAttackCount = true;
    public const bool NegatedAttackCountsForTrait = true;
}
