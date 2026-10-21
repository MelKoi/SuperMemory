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

    // （对应）和（连击）都是同一张牌的另一行效果，不另占携带数。
    public const bool ResponseRowConsumesCarryLimit = false;
    public const bool ComboRowConsumesCarryLimit = false;

    // 结算时已经处于连击，并且存在同名（连击）行，就用那一行。
    public const bool ComboRowReplacesBaseRow = true;

    // 限制牌不能打出。蓄能是弃一张手牌，指定一把武器，蓄能 +1，限制牌可以这样用。
    public const bool RestrictedCardCanBePlayed = false;
    public const bool RestrictedCardCanCharge = true;
    public const int ChargeGainPerCard = 1;

    // 表里没有攻击次数的列。每回合 1 次武器攻击，用完不能宣告。迅捷的第一次不消耗这次次数。
    public const int AttacksPerTurn = 1;

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

    // 手牌里的临时牌在回合结束时移出本局。变成滞留之后留到下一次攻击，不在回合结束清除。
    public const bool RemoveTemporaryFromHand = true;
    public const bool ClearLingerAtTurnEnd = false;

    // 只统计自己主要阶段里打出的牌和自己宣告的武器攻击。对方回合的对应不计数。
    public const bool ResponseCountsAsOwnTraitAction = false;

    // 攻击消耗该武器当前全部蓄能。四列顺序见 WeaponChargeTier。
    public const bool AttackSpendsEntireCharge = true;
}
