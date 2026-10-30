using System;

// 起手能量。先手 3 点，后手 5 点。这里不加上限。
public static class OpeningEnergyGrant
{
    public static void Grant(FighterState player, FighterState enemy, BattleSide firstPlayer)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (enemy == null)
        {
            throw new ArgumentNullException(nameof(enemy));
        }

        var playerFirst = firstPlayer == player.Side;
        player.Energy = playerFirst ? MatchTimingPolicy.FirstPlayerEnergy : MatchTimingPolicy.SecondPlayerEnergy;
        enemy.Energy = playerFirst ? MatchTimingPolicy.SecondPlayerEnergy : MatchTimingPolicy.FirstPlayerEnergy;
    }
}
