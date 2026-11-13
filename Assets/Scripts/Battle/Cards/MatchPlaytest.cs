using System;

// 战斗测试的入场。局前和开局已经在流程外做完，这里从先手回合开始走到我方主要阶段。
// 先手的第一个主要阶段直接结束，不询问洗牌。不调用 Begin，避免把局前和开局再走一遍。
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

        var rules = MatchRules.Create(cards, player, enemy);
        rules.OpenAtTurnStart(firstPlayer);
        MatchCycle.PassShuffle(rules, random);
        rules.EndMainPhase();
        MatchCycle.ContinueUntil(rules, BattleSide.Player, random);
        return rules;
    }
}
