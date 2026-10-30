public static class MatchTimingPolicy
{
    public const int OpeningHandCount = 3;
    public const int FirstPlayerEnergy = 3;
    public const int SecondPlayerEnergy = 5;
    public const int TurnEnergyGain = 5;
    public const int ShuffleLifeCost = 1;
    public const int BoardColumns = 12;
    public const int BoardRows = 3;
    public const int OpeningDistance = 10;

    // 表里没有这些数。流程不读取它们，定值之前不要当成正式规则。
    public const int PlaceholderMoveEnergyCost = 0;
    public const int PlaceholderToughnessRecovery = 0;
    public const int PlaceholderVulnerabilityNumerator = 1;
    public const int PlaceholderVulnerabilityDenominator = 1;

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
