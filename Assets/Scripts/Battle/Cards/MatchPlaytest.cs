using System;

// 战斗测试的入场。局前和开局已经在流程外做完，这里从先手回合走到我方主要阶段。
// 不调用 MatchFlow.Begin，避免把已经走过的局前和开局再走一遍。
public static class MatchPlaytest
{
    public static MatchRules ReachPlayerMain(
        CardCatalog cards,
        FighterState player,
        FighterState enemy,
        BattleSide firstPlayer,
        Random random)
    {
        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (enemy == null)
        {
            throw new ArgumentNullException(nameof(enemy));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var active = firstPlayer == BattleSide.Player ? player : enemy;
        var waiting = firstPlayer == BattleSide.Player ? enemy : player;
        TurnToMain.Advance(active, waiting, cards, random);
        var rules = MatchRules.Create(cards, player, enemy);
        rules.OpenAtMain(firstPlayer);
        rules.EndMainPhase();
        MatchCycle.ContinueUntil(rules, BattleSide.Player, random);
        return rules;
    }
}
