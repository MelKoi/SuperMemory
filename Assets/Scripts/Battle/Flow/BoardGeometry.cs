using System;
using System.Globalization;

// 距离是两格中心的直线长度，向下取整后再减 1。相邻一格是 0，同一格也是 0。
// 场地长边 12 格、短边 3 条路。场景里 BoardCell.Row 是长边，Column 是路。
public static class BoardGeometry
{
    public static int Distance(int columnA, int rowA, int columnB, int rowB)
    {
        var dx = columnA - columnB;
        var dy = rowA - rowB;
        var squared = dx * (long)dx + dy * (long)dy;
        var cells = FloorSqrt(squared) - 1;
        return cells < 0 ? 0 : cells;
    }

    static int FloorSqrt(long value)
    {
        if (value <= 0)
        {
            return 0;
        }

        var root = (long)Math.Sqrt(value);
        while ((root + 1) * (root + 1) <= value)
        {
            root++;
        }

        while (root * root > value)
        {
            root--;
        }

        return (int)root;
    }
}

public static class OpeningPlacement
{
    public static SeatingResult Create(BattleSide firstPlayer)
    {
        var lane = (MatchTimingPolicy.BoardRows - 1) / 2;
        var farRow = MatchTimingPolicy.BoardColumns - 1;
        var distance = BoardGeometry.Distance(lane, 0, lane, farRow);
        if (distance != MatchTimingPolicy.OpeningDistance)
        {
            throw new InvalidOperationException(
                "开局站位距离是 " + distance.ToString(CultureInfo.InvariantCulture)
                + "，不是 " + MatchTimingPolicy.OpeningDistance.ToString(CultureInfo.InvariantCulture) + "。");
        }

        return new SeatingResult(firstPlayer, lane, 0, lane, farRow, distance);
    }
}

public sealed class SeatingResult
{
    public SeatingResult(BattleSide firstPlayer, int playerColumn, int playerRow, int enemyColumn, int enemyRow, int distance)
    {
        FirstPlayer = firstPlayer;
        PlayerColumn = playerColumn;
        PlayerRow = playerRow;
        EnemyColumn = enemyColumn;
        EnemyRow = enemyRow;
        Distance = distance;
    }

    public BattleSide FirstPlayer { get; }
    public BattleSide SecondPlayer => FirstPlayer == BattleSide.Player ? BattleSide.Enemy : BattleSide.Player;
    public int PlayerColumn { get; }
    public int PlayerRow { get; }
    public int EnemyColumn { get; }
    public int EnemyRow { get; }
    public int Distance { get; }
}
