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

    // （对应）是同一张牌在窗口里的效果行，不另占携带数。
    public const bool ResponseRowConsumesCarryLimit = false;

    // 一次攻击的窗口里按顺序结算任意张合法对应，全部完成后再结算伤害。
    public const bool AnyNumberOfResponsesBeforeDamage = true;

    // 攻击被打消：伤害和滞留附加不生效，滞留仍消耗。蓄能、攻击次数照常消耗，这次宣告仍计入特质。
    public const bool NegatedAttackDealsDamage = false;
    public const bool NegatedAttackAppliesLinger = false;
    public const bool NegatedAttackConsumesLinger = true;
    public const bool NegatedAttackSpendsCharge = true;
    public const bool NegatedAttackSpendsAttackCount = true;
    public const bool NegatedAttackCountsForTrait = true;

    // 滞留绑在玩家身上，下一次任一武器攻击消耗。
    public const bool LingerFollowsPlayer = true;

    // 回合结束时，手牌里和还没结算的滞留里的临时牌移出本局。
    public const bool RemoveTemporaryFromHandAndPendingLinger = true;

    // 只统计自己主要阶段里打出的牌和自己宣告的武器攻击。对方回合的对应不计数。
    public const bool ResponseCountsAsOwnTraitAction = false;

    // 攻击消耗该武器当前全部蓄能。武器四元组按「蓄能门槛、生命伤害、韧性伤害、攻击距离」读。
    public const bool AttackSpendsEntireCharge = true;
}
