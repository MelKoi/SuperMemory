// 一段效果是 效果号, 参数..., 目标。目标 0 是打出者自己，1 是对方。
// 读表时已经检查过长度和目标，结算时直接按位置取参数。
public static class CardEffects
{
    public const int None = -1;
    public const int Draw = 0;
    public const int Energy = 1;
    public const int Toughness = 2;
    public const int Life = 3;
    public const int Strike = 4;
    public const int AddCard = 5;
    public const int Range = 6;
    public const int Cost = 7;
    public const int CannotRespond = 8;
    public const int Discard = 9;
    public const int Status = 10;
    public const int Negate = 11;
    public const int RemoveSubtype = 12;
    public const int DiscardOrNegate = 13;

    public const int TargetSelf = 0;
    public const int TargetOpponent = 1;

    // 效果号加上参数的总长度，最后一位都是目标。
    public static int Length(int effect)
    {
        switch (effect)
        {
            case Draw:
            case Energy:
            case Toughness:
            case Life:
            case Range:
            case Cost:
            case Discard:
            case Status:
            case RemoveSubtype:
            case DiscardOrNegate:
                return 3;
            case Strike:
                return 4;
            case AddCard:
                return 5;
            case CannotRespond:
            case Negate:
                return 2;
            default:
                return -1;
        }
    }

    public static bool NeedsAttack(int effect)
    {
        return effect == Negate || effect == DiscardOrNegate;
    }

    public static bool ModifiesAttack(int effect)
    {
        return effect == Strike || effect == Range || effect == CannotRespond;
    }

    public static bool IsEmpty(int[] step)
    {
        return step == null || step.Length == 0 || step[0] == None;
    }

    public static BattleSide TargetSide(BattleSide source, int[] step)
    {
        return step[step.Length - 1] == TargetSelf ? source : MatchFlow.Opposite(source);
    }

    public static bool AnyNeedsAttack(int[][] effects)
    {
        for (var i = 0; i < effects.Length; i++)
        {
            if (!IsEmpty(effects[i]) && NeedsAttack(effects[i][0]))
            {
                return true;
            }
        }

        return false;
    }
}
