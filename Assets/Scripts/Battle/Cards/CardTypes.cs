using System.IO;

// 三大类。表号和枚举值一致：0 攻击牌表，1 行动牌表，2 加成牌表。
public enum CardKind
{
    Attack = 0,
    Action = 1,
    Buff = 2,
}

public static class CardKinds
{
    public static CardKind FromTable(int table)
    {
        if (table < (int)CardKind.Attack || table > (int)CardKind.Buff)
        {
            throw new InvalidDataException("未知卡牌表号：" + table);
        }

        return (CardKind)table;
    }
}

public static class CardSubtypes
{
    public const int None = 0;
    public const int Response = 1;
    public const int AllOut = 2;
    public const int Temporary = 3;
    public const int Restricted = 4;
}

public static class CardStatuses
{
    public const int Combo = 0;
    public const int Swift = 1;
}
