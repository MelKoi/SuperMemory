using CardTables;
using Xunit;

public class CardRulesTests
{
    static readonly string TableDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Assets", "StreamingAssets", "Tables");

    [Fact]
    public void DeckBuild_UsesGlovesAndSword_AndDoesNotDraw()
    {
        var cards = LoadCards();
        Assert.Equal(10, DeckBuilder.Collect(cards, "c_0").Count);
        Assert.Equal(11, DeckBuilder.Collect(cards, "w_0").Count);
        Assert.Equal(11, DeckBuilder.Collect(cards, "w_1").Count);

        var gloves = DeckBuilder.Collect(cards, "w_0");
        Assert.Equal(2, gloves.FindAll(card => card.Kind == CardKind.Action && card.CardId == 1).Count);
        Assert.DoesNotContain(gloves, card => card.Kind == CardKind.Action && card.CardId == 2);
        Assert.DoesNotContain(gloves, card => card.Kind == CardKind.Attack && (card.CardId == 1 || card.CardId == 2));
        var sword = DeckBuilder.Collect(cards, "w_1");
        Assert.DoesNotContain(sword, card => card.Kind == CardKind.Action && card.CardId == 5);
        var rows = new List<CardDefinition>();
        cards.CopyTo(rows);
        foreach (var owner in new[] { "c_0", "w_0", "w_1" })
        {
            foreach (var card in DeckBuilder.Collect(cards, owner))
            {
                var definition = cards.Get(card.Kind, card.CardId);
                Assert.False(DeckBuilder.HasNameRemark(definition.Name), definition.Name);
            }
        }

        Assert.Contains(rows, card => DeckBuilder.HasNameRemark(card.Name) && card.MaxTake > 0);

        var built = DeckBuilder.Build(cards, 0);
        var summary = string.Join("\n", DeckBuilder.Summarize(cards, built.Deck));
        Assert.DoesNotContain("(对应)", summary);
        Assert.DoesNotContain("(连击)", summary);
        Assert.DoesNotContain("（对应）", summary);
        Assert.DoesNotContain("（连击）", summary);
        Assert.Contains("臂格挡 ×2", summary);
        var again = DeckBuilder.Build(cards, 0);
        Assert.Equal(DeckBuilder.GlovesId, built.WeaponId1);
        Assert.Equal(DeckBuilder.GreatswordId, built.WeaponId2);
        Assert.Equal(32, built.Deck.Count);
        Assert.Equal(
            built.Deck.Select(card => (card.Kind, card.CardId)),
            again.Deck.Select(card => (card.Kind, card.CardId)));
        Assert.Throws<ArgumentException>(() => DeckBuilder.Build(cards, 0, 0, 0));

        var player = DeckBuilder.CreatePlayer(
            built,
            4,
            new WeaponLoadout(0, "w_0", default, default),
            new WeaponLoadout(1, "w_1", default, default));
        Assert.Empty(player.Hand);
        Assert.Equal(32, player.Deck.Count);
        Assert.Equal(2, player.Weapons.Count);
    }

    [Fact]
    public void OpeningSeats_StandAtTheEnds_WithDistance8()
    {
        Assert.Equal(0, BoardGeometry.Distance(1, 0, 1, 0));
        Assert.Equal(0, BoardGeometry.Distance(1, 0, 1, 1));
        Assert.Equal(1, BoardGeometry.Distance(1, 0, 1, 2));
        Assert.Equal(0, BoardGeometry.Distance(0, 0, 1, 1));
        Assert.Equal(4, BoardGeometry.Distance(0, 0, 3, 4));

        var playerFirst = OpeningPlacement.Create(BattleSide.Player);
        var enemyFirst = OpeningPlacement.Create(BattleSide.Enemy);
        Assert.Equal(BattleSide.Player, playerFirst.FirstPlayer);
        Assert.Equal(BattleSide.Enemy, playerFirst.SecondPlayer);
        Assert.Equal(BattleSide.Player, enemyFirst.SecondPlayer);
        Assert.Equal(1, playerFirst.PlayerColumn);
        Assert.Equal(0, playerFirst.PlayerRow);
        Assert.Equal(1, playerFirst.EnemyColumn);
        Assert.Equal(9, playerFirst.EnemyRow);
        Assert.Equal(MatchTimingPolicy.OpeningDistance, playerFirst.Distance);
        Assert.Equal(8, playerFirst.Distance);
        Assert.Equal(playerFirst.Distance, enemyFirst.Distance);
        Assert.Equal(
            playerFirst.Distance,
            BoardGeometry.Distance(playerFirst.PlayerColumn, playerFirst.PlayerRow, playerFirst.EnemyColumn, playerFirst.EnemyRow));
    }

    [Fact]
    public void OpeningHand_DrawsToThree_AndIgnoresHandLimit()
    {
        var limited = new FighterState(BattleSide.Player, "c_0", 0);
        limited.Energy = 4;
        for (var i = 0; i < 5; i++)
        {
            limited.Deck.Add(new CardInstance(CardKind.Action, i));
        }

        OpeningHand.DrawTo(limited, MatchTimingPolicy.OpeningHandCount);
        Assert.Equal(3, limited.Hand.Count);
        Assert.Equal(2, limited.Deck.Count);
        Assert.Equal(0, limited.Hand[0].CardId);
        Assert.Equal(1, limited.Hand[1].CardId);
        Assert.Equal(2, limited.Hand[2].CardId);
        Assert.Equal(3, limited.Deck[0].CardId);
        Assert.Equal(4, limited.Energy);

        var roomy = new FighterState(BattleSide.Player, "c_1", 4);
        roomy.Deck.Add(new CardInstance(CardKind.Attack, 8));
        roomy.Deck.Add(new CardInstance(CardKind.Attack, 9));
        OpeningHand.DrawTo(roomy, MatchTimingPolicy.OpeningHandCount);
        Assert.Equal(2, roomy.Hand.Count);
        Assert.Empty(roomy.Deck);

        var full = new FighterState(BattleSide.Enemy, string.Empty, 0);
        full.Hand.Add(new CardInstance(CardKind.Buff, 1));
        full.Hand.Add(new CardInstance(CardKind.Buff, 2));
        full.Hand.Add(new CardInstance(CardKind.Buff, 3));
        full.Hand.Add(new CardInstance(CardKind.Buff, 4));
        full.Deck.Add(new CardInstance(CardKind.Buff, 5));
        OpeningHand.DrawTo(full, MatchTimingPolicy.OpeningHandCount);
        Assert.Equal(4, full.Hand.Count);
        Assert.Single(full.Deck);

        var empty = new FighterState(BattleSide.Enemy, string.Empty, 0);
        OpeningHand.DrawTo(empty, MatchTimingPolicy.OpeningHandCount);
        Assert.Empty(empty.Hand);
    }

    [Fact]
    public void OpeningHand_ShufflesBothDecks_BeforeDrawing()
    {
        var first = DeckOf(0, 1, 2, 3, 4, 5, 6, 7);
        var second = DeckOf(0, 1, 2, 3, 4, 5, 6, 7);
        first.Hand.Add(new CardInstance(CardKind.Buff, 9));
        OpeningHand.Shuffle(first, new Random(7));
        OpeningHand.Shuffle(second, new Random(7));
        Assert.Equal(first.Deck.Select(card => card.CardId), second.Deck.Select(card => card.CardId));
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, first.Deck.Select(card => card.CardId).OrderBy(id => id));
        Assert.Equal(9, first.Hand[0].CardId);

        var changed = false;
        for (var seed = 1; seed <= 30; seed++)
        {
            var trial = DeckOf(0, 1, 2, 3, 4, 5, 6, 7);
            OpeningHand.Shuffle(trial, new Random(seed));
            if (!trial.Deck.Select(card => card.CardId).SequenceEqual(new[] { 0, 1, 2, 3, 4, 5, 6, 7 }))
            {
                changed = true;
                break;
            }
        }

        Assert.True(changed);

        var player = DeckOf(0, 1, 2, 3, 4, 5, 6, 7);
        var enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        var random = new Random(11);
        OpeningHand.Shuffle(player, random);
        OpeningHand.Shuffle(enemy, random);
        var top = player.Deck.Take(3).Select(card => card.CardId).ToArray();
        OpeningHand.DrawTo(player, MatchTimingPolicy.OpeningHandCount);
        OpeningHand.DrawTo(enemy, MatchTimingPolicy.OpeningHandCount);
        Assert.Equal(top, player.Hand.Select(card => card.CardId));
        Assert.Empty(enemy.Hand);
        Assert.Empty(enemy.Deck);
    }

    [Fact]
    public void OpeningEnergy_GivesThreeToFirst_AndFiveToSecond()
    {
        var player = new FighterState(BattleSide.Player, "c_0", 4);
        var enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        OpeningEnergyGrant.Grant(player, enemy, BattleSide.Player);
        Assert.Equal(MatchTimingPolicy.FirstPlayerEnergy, player.Energy);
        Assert.Equal(MatchTimingPolicy.SecondPlayerEnergy, enemy.Energy);
        Assert.Equal(3, player.Energy);
        Assert.Equal(5, enemy.Energy);
        Assert.Single(player.Hand);

        OpeningEnergyGrant.Grant(player, enemy, BattleSide.Enemy);
        Assert.Equal(5, player.Energy);
        Assert.Equal(3, enemy.Energy);
    }

    [Fact]
    public void FirstTurn_TicksExpiresGivesEnergyAndRecoversToughness()
    {
        var cards = LoadCards();
        var heart = cards.Get(CardKind.Buff, 0);
        Assert.True(heart.EndEffect == null || heart.EndEffect.Length == 0 || heart.EndEffect[0][0] == CardEffects.None);

        var active = new FighterState(BattleSide.Player, "c_0", 4);
        var other = new FighterState(BattleSide.Enemy, string.Empty, 0);
        active.Energy = 3;
        other.Energy = 5;
        active.SetHealth(20);
        active.SetToughness(18);
        other.SetToughness(17);
        var staying = new CardInstance(CardKind.Buff, 0);
        staying.Duration = 4;
        active.Field.Add(staying);
        var ending = new CardInstance(CardKind.Buff, 0);
        ending.Duration = 1;
        other.Field.Add(ending);
        for (var i = 0; i < 4; i++)
        {
            active.Deck.Add(new CardInstance(CardKind.Action, i));
        }

        var before = active.Deck.Select(card => card.CardId).ToArray();
        var rules = MatchRules.Create(cards, active, other);
        rules.OpenAtTurnStart(BattleSide.Player);
        MatchCycle.PassShuffle(rules, new Random(3));
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
        Assert.False(rules.Flow.LastShuffleDidShuffle);
        Assert.Equal(3, staying.Duration);
        Assert.Single(active.Field);
        Assert.Empty(other.Field);
        Assert.Same(ending, Assert.Single(other.Discard));
        Assert.Equal(before, active.Deck.Select(card => card.CardId));
        Assert.Equal(8, active.Energy);
        Assert.Equal(5, other.Energy);
        Assert.Equal(20, active.Health);
        Assert.Equal(19, active.Toughness);
        Assert.Equal(17, other.Toughness);

        var empty = new FighterState(BattleSide.Enemy, string.Empty, 0);
        var idle = new FighterState(BattleSide.Player, "c_0", 4);
        empty.Energy = 3;
        empty.SetHealth(20);
        empty.Discard.Add(new CardInstance(CardKind.Action, 4));
        empty.Discard.Add(new CardInstance(CardKind.Action, 5));
        var forced = MatchRules.Create(cards, idle, empty);
        forced.OpenAtTurnStart(BattleSide.Enemy);
        MatchCycle.PassShuffle(forced, new Random(1));
        Assert.True(forced.Flow.LastShuffleDidShuffle);
        Assert.Equal(2, empty.Deck.Count);
        Assert.Empty(empty.Discard);
        Assert.Equal(8, empty.Energy);
        Assert.Equal(20, empty.Health);
    }

    [Fact]
    public void OpenAtMain_StopsInMain()
    {
        var flow = new MatchFlow();
        flow.OpenAtMain(BattleSide.Enemy);
        Assert.Equal(MatchTiming.Main, flow.Timing);
        Assert.True(flow.IsWaiting);
        Assert.Equal(BattleSide.Enemy, flow.ActiveSide);
        Assert.Equal(1, flow.AttacksRemaining);
        flow.CompleteCharge(0);
        Assert.Equal(MatchTiming.Main, flow.Timing);
        Assert.Throws<InvalidOperationException>(() => flow.OpenAtMain(BattleSide.Player));
    }

    [Fact]
    public void FirstMain_EndsIntoTheSecondPlayersMain()
    {
        var cards = LoadCards();
        var player = new FighterState(BattleSide.Player, "c_0", 4);
        var enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        player.Energy = 8;
        enemy.Energy = 5;
        var temporary = new CardInstance(CardKind.Attack, 8);
        player.Hand.Add(temporary);
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        player.Hand.Add(new CardInstance(CardKind.Action, 2));
        for (var i = 0; i < 6; i++)
        {
            player.Deck.Add(new CardInstance(CardKind.Action, 20 + i));
        }

        var rules = MatchRules.Create(cards, player, enemy);
        rules.OpenAtMain(BattleSide.Player);
        rules.EndMainPhase();
        Assert.Equal(MatchTiming.Shuffle, rules.Flow.Timing);
        Assert.Equal(BattleSide.Enemy, rules.Flow.ActiveSide);
        Assert.Equal(4, player.Hand.Count);
        Assert.DoesNotContain(player.Hand, card => ReferenceEquals(card, temporary));
        Assert.Contains(player.Removed, card => ReferenceEquals(card, temporary));
        Assert.Equal(4, player.Deck.Count);
        Assert.Equal(5, enemy.Energy);

        rules.Flow.ResolveShuffle(false);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
        Assert.True(rules.Flow.IsWaiting);
        Assert.Equal(BattleSide.Enemy, rules.Flow.ActiveSide);
        Assert.Equal(8, player.Energy);
    }

    [Fact]
    public void SecondRound_ReturnsToTheFirstPlayersMain()
    {
        var cards = LoadCards();
        var player = new FighterState(BattleSide.Player, "c_0", 4);
        var enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        player.Energy = 8;
        enemy.Energy = 5;
        var rules = MatchRules.Create(cards, player, enemy);
        var random = new Random(1);
        rules.OpenAtMain(BattleSide.Player);
        rules.EndMainPhase();
        MatchCycle.ContinueUntil(rules, BattleSide.Player, random);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
        Assert.True(rules.Flow.IsWaiting);
        Assert.Equal(BattleSide.Player, rules.Flow.ActiveSide);
        Assert.Equal(3, rules.Flow.TurnIndex);
        Assert.Equal(13, player.Energy);
        Assert.Equal(10, enemy.Energy);

        rules.EndMainPhase();
        MatchCycle.ContinueUntil(rules, BattleSide.Player, random);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
        Assert.True(rules.Flow.IsWaiting);
        Assert.Equal(BattleSide.Player, rules.Flow.ActiveSide);
        Assert.Equal(5, rules.Flow.TurnIndex);
        Assert.Equal(18, player.Energy);
        Assert.Equal(15, enemy.Energy);
    }

    [Fact]
    public void ShuffleChoice_WaitsWhenTheDeckHasCards()
    {
        var cards = LoadCards();
        var player = new FighterState(BattleSide.Player, "c_0", 0);
        var enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        player.Energy = 8;
        player.Deck.Add(new CardInstance(CardKind.Action, 1));
        player.Deck.Add(new CardInstance(CardKind.Action, 2));
        player.Deck.Add(new CardInstance(CardKind.Action, 3));
        var rules = MatchRules.Create(cards, player, enemy);
        var random = new Random(1);
        rules.OpenAtMain(BattleSide.Player);
        rules.EndMainPhase();

        var waiting = MatchCycle.ContinueUntil(rules, BattleSide.Player, random);
        Assert.True(waiting.WaitingForShuffle);
        Assert.Equal(MatchTiming.Shuffle, rules.Flow.Timing);
        Assert.Equal(BattleSide.Player, rules.Flow.ActiveSide);
        Assert.Equal(20, player.Health);
        Assert.Equal(8, player.Energy);
        Assert.Equal(new[] { 1, 2, 3 }, player.Deck.Select(card => card.CardId));

        var declined = MatchCycle.AnswerShuffle(rules, BattleSide.Player, false, random);
        Assert.False(declined.WaitingForShuffle);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
        Assert.False(rules.Flow.LastShuffleDidShuffle);
        Assert.Equal(20, player.Health);
        Assert.Equal(13, player.Energy);
        Assert.Equal(new[] { 1, 2, 3 }, player.Deck.Select(card => card.CardId));
    }

    [Fact]
    public void ShuffleChoice_YesPaysOneLife_AndShufflesOnlyTheDeck()
    {
        var cards = LoadCards();
        var player = new FighterState(BattleSide.Player, "c_0", 0);
        var enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        player.Energy = 8;
        player.Deck.Add(new CardInstance(CardKind.Action, 1));
        player.Deck.Add(new CardInstance(CardKind.Action, 2));
        player.Deck.Add(new CardInstance(CardKind.Action, 3));
        var rules = MatchRules.Create(cards, player, enemy);
        rules.OpenAtMain(BattleSide.Player);
        rules.EndMainPhase();
        Assert.True(MatchCycle.ContinueUntil(rules, BattleSide.Player, new Random(1)).WaitingForShuffle);

        player.SetHealth(1);
        var finished = MatchCycle.AnswerShuffle(rules, BattleSide.Player, true, new Random(1));
        Assert.False(finished.WaitingForShuffle);
        Assert.True(rules.Flow.IsFinished);
        Assert.Equal(0, player.Health);
        Assert.Equal(8, player.Energy);

        var again = MatchRules.Create(cards, DeckWithThree(), enemy);
        again.OpenAtMain(BattleSide.Player);
        again.EndMainPhase();
        Assert.True(MatchCycle.ContinueUntil(again, BattleSide.Player, new Random(1)).WaitingForShuffle);
        var accepted = MatchCycle.AnswerShuffle(again, BattleSide.Player, true, new Random(4));
        Assert.False(accepted.WaitingForShuffle);
        Assert.Equal(MatchTiming.Main, again.Flow.Timing);
        Assert.True(again.Flow.LastShuffleDidShuffle);
        Assert.Equal(19, again.Player.Health);
        Assert.Equal(13, again.Player.Energy);
        Assert.Equal(3, again.Player.Deck.Count);
        Assert.Empty(again.Player.Discard);
    }

    [Fact]
    public void Move_RecordsAMainAction_WithoutSpendingEnergy()
    {
        var cards = LoadCards();
        var player = new FighterState(BattleSide.Player, "c_0", 4);
        var enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        player.Energy = 4;
        var rules = MatchRules.Create(cards, player, enemy);
        rules.OpenAtMain(BattleSide.Player);

        Assert.True(rules.TryMove(BattleSide.Player, out var error), error);
        Assert.True(rules.TryMove(BattleSide.Player, out error), error);
        Assert.Equal(4, player.Energy);
        Assert.Equal(2, rules.Flow.MainActionCount);
        Assert.Equal(0, rules.Flow.TraitActionsThisTurn);
    }

    static FighterState DeckWithThree()
    {
        var player = new FighterState(BattleSide.Player, "c_0", 0);
        player.Energy = 8;
        player.Deck.Add(new CardInstance(CardKind.Action, 1));
        player.Deck.Add(new CardInstance(CardKind.Action, 2));
        player.Deck.Add(new CardInstance(CardKind.Action, 3));
        return player;
    }

    static FighterState DeckOf(params int[] cardIds)
    {
        var fighter = new FighterState(BattleSide.Player, "c_0", 0);
        for (var i = 0; i < cardIds.Length; i++)
        {
            fighter.Deck.Add(new CardInstance(CardKind.Action, cardIds[i]));
        }

        return fighter;
    }

    [Fact]
    public void Catalog_LinksComboAndResponseRows()
    {
        var cards = LoadCards();
        var eyes = cards.Get(CardKind.Action, 7);
        Assert.Equal("千眼", eyes.Name);
        Assert.Equal(8, eyes.Combo.Id);
        Assert.Equal(9, eyes.Combo.PlayEffect[0][2]);

        var block = cards.Get(CardKind.Action, 1);
        Assert.Equal(2, block.Response.Id);
        Assert.Equal(3, block.Response.Cost);
        Assert.Equal(2, block.Cost);
        Assert.Equal("w_0", block.Owner);
        Assert.Equal("w_0", block.Response.Owner);

        var heart = cards.Get(CardKind.Buff, 0);
        Assert.Equal(1, heart.Combo.Id);
        Assert.Equal(4, heart.Duration);
    }

    [Fact]
    public void ActionCard_ResolvesImmediately_AndSwiftStaysForTheTurn()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 4;
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(0, player.Energy);
        Assert.Equal(2, player.Hand.Count);
        Assert.Equal(12, player.Hand[0].CardId);
        Assert.Equal(CardKind.Action, player.Hand[0].Kind);
        Assert.True(player.HasStatus(CardStatuses.Swift));
        Assert.Equal(0, player.Discard[0].CardId);

        player.Weapons[0].Charge = 1;
        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out error), error);
        rules.CloseResponseWindow();
        Assert.Equal(1, rules.Flow.AttacksRemaining);
    }

    [Fact]
    public void ActionCard_UsesComboRowWithoutChangingCost()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 2;
        player.AddStatus(CardStatuses.Combo);
        player.Hand.Add(new CardInstance(CardKind.Action, 7));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(0, player.Energy);
        Assert.Equal(CardKind.Attack, player.Hand[0].Kind);
        Assert.Equal(9, player.Hand[0].CardId);
        Assert.Equal(CardSubtypes.None, rules.Cards.Get(CardKind.Attack, 9).Subtype);
    }

    [Fact]
    public void ActionCard_WithoutCombo_AddsTheTemporaryVersion()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 2;
        player.Hand.Add(new CardInstance(CardKind.Action, 7));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(8, player.Hand[0].CardId);
        Assert.Equal(CardSubtypes.Temporary, rules.Cards.Get(CardKind.Attack, 8).Subtype);
    }

    [Fact]
    public void AttackCard_LingersUntilTheNextWeaponAttack_AndTemporaryLeavesAtTurnEnd()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Energy = 2;
        player.Weapons[0].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Attack, 0));
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(1, player.Hand[0].CardId);
        Assert.Single(player.Linger);
        Assert.Equal(0, player.Discard[0].CardId);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out error), error);
        rules.CloseResponseWindow();
        Assert.Equal(18, enemy.Health);
        Assert.Equal(17, enemy.Toughness);
        Assert.Empty(player.Linger);
        Assert.Equal(0, player.Weapons[0].Charge);

        rules.EndMainPhase();
        Assert.Empty(player.Hand);
        Assert.Equal(1, player.Removed[0].CardId);
    }

    [Fact]
    public void AttackCard_NextAttackCanGrantEnergy()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Energy = 2;
        player.Weapons[1].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Attack, 6));
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(0, player.Energy);
        Assert.True(rules.TryAttack(BattleSide.Player, 1, 0, out error), error);
        rules.CloseResponseWindow();
        Assert.Equal(3, player.Energy);
        WeaponChargeTier.Resolve(1, player.Weapons[1].Light, player.Weapons[1].Heavy, out var life, out _, out _);
        Assert.Equal(20 - life, enemy.Health);
    }

    [Fact]
    public void AttackCard_CannotRespondSkipsTheWindow()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Energy = 2;
        player.Weapons[0].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Attack, 8));
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out error), error);
        Assert.False(rules.Flow.AttackInProgress);
        Assert.Contains(rules.Flow.LastAttack.Steps, step => step.Timing == AttackTiming.ResponseWindow && step.Skipped);
        Assert.Equal(18, enemy.Health);
        Assert.Equal(17, enemy.Toughness);
    }

    [Fact]
    public void ResponseRow_ChangesCostAndCanNegateWhenDiscardFails()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Weapons[1].Charge = 1;
        player.Energy = 1;
        player.Hand.Add(new CardInstance(CardKind.Attack, 4));
        enemy.Energy = 3;
        enemy.Hand.Add(new CardInstance(CardKind.Action, 1));
        enemy.Weapons.Add(Weapon(0, "w_0"));
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.True(rules.TryAttack(BattleSide.Player, 1, 0, out error), error);
        Assert.True(rules.TryPlay(BattleSide.Enemy, 0, true, out error), error);
        Assert.Equal(0, enemy.Energy);
        rules.CloseResponseWindow();
        Assert.Equal(20, enemy.Health);
        Assert.Equal(20, enemy.Toughness);
        Assert.Equal(0, player.Weapons[1].Charge);
        Assert.Empty(player.Linger);
    }

    [Fact]
    public void ResponseRow_ModifiesTheAttackBeforeDamage()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Weapons[0].Charge = 1;
        enemy.Energy = 2;
        enemy.Hand.Add(new CardInstance(CardKind.Action, 4));
        enemy.Weapons.Add(new WeaponLoadout(1, "w_1", default, default));
        var rules = Start(player, enemy);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        Assert.True(rules.TryPlay(BattleSide.Enemy, 0, true, out error), error);
        rules.CloseResponseWindow();
        Assert.Equal(18, enemy.Health);
        Assert.Equal(20, enemy.Toughness);
    }

    [Fact]
    public void Block_PlayedInMain_UsesTheBaseRow()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 2;
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        var rules = Start(player, NewFighter(BattleSide.Enemy));
        player.SetToughness(10);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(0, player.Energy);
        Assert.Equal(12, player.Toughness);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
    }

    [Fact]
    public void BuffCard_TurnStartTicksBothSides_PlayResolvesStart()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        var enemyBuff = new CardInstance(CardKind.Buff, 0);
        enemyBuff.Duration = 4;
        enemy.Field.Add(enemyBuff);
        var rules = MatchRules.Create(LoadCards(), player, enemy);
        rules.Begin(BattleSide.Player);

        Assert.Equal(3, enemyBuff.Duration);
        Assert.Empty(enemy.Hand);
        rules.Flow.ResolveShuffle(false);

        player.Energy = 3;
        player.Hand.Add(new CardInstance(CardKind.Buff, 0));
        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(1, player.Energy);
        Assert.Empty(player.Hand);
        Assert.Equal(4, player.Field[0].Duration);
    }

    [Fact]
    public void BuffCard_OngoingAddsTheEnergyCardAtTurnEnd()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 3;
        player.Hand.Add(new CardInstance(CardKind.Buff, 0));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(1, player.Energy);
        Assert.Equal(4, player.Field[0].Duration);
        Assert.Empty(player.Hand);
        rules.EndMainPhase();
        Assert.Equal(3, player.Field[0].Duration);
        Assert.Equal(13, Assert.Single(player.Hand).CardId);

        player.Field[0].Duration = 1;
        rules.Flow.ResolveShuffle(false);
        rules.EndMainPhase();
        Assert.Empty(player.Field);
        Assert.Equal(0, player.Discard[0].CardId);
        Assert.Equal(13, Assert.Single(player.Hand).CardId);
    }

    [Fact]
    public void BuffCard_OngoingUsesTheComboRowWhenComboIsActive()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 3;
        player.AddStatus(CardStatuses.Combo);
        player.Hand.Add(new CardInstance(CardKind.Buff, 0));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(1, player.Field[0].EffectCardId);
        Assert.Equal(1, player.Energy);
        Assert.Empty(player.Hand);
        rules.EndMainPhase();
        Assert.Equal(2, player.Hand.Count);
        Assert.Equal(13, player.Hand[0].CardId);
        Assert.Equal(13, player.Hand[1].CardId);
        Assert.Equal(3, player.Field[0].Duration);
    }

    [Fact]
    public void AllOut_EndsTheMainPhase_AndComboDealsTheExtraLife()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Energy = 3;
        player.Hand.Add(new CardInstance(CardKind.Action, 10));
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(19, enemy.Health);
        Assert.False(player.HasStatus(CardStatuses.Combo));
        Assert.Equal(MatchTiming.Shuffle, rules.Flow.Timing);
        Assert.Equal(BattleSide.Enemy, rules.Flow.ActiveSide);
        Assert.Equal(MainEndReason.AllOut, rules.Flow.MainEndReason);
    }

    [Fact]
    public void AllOut_SecondActionIsRejected()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 5;
        player.Hand.Add(new CardInstance(CardKind.Action, 3));
        player.Hand.Add(new CardInstance(CardKind.Action, 10));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.False(rules.TryPlay(BattleSide.Player, 0, false, out error));
        Assert.Equal("全力必须是主要阶段的第一个行动。", error);
        Assert.Equal(10, player.Hand[0].CardId);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
    }

    [Fact]
    public void RestrictedCard_CannotBePlayed()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 4;
        player.Hand.Add(new CardInstance(CardKind.Action, 12));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.False(rules.TryPlay(BattleSide.Player, 0, false, out var error));
        Assert.Equal("限制牌不能打出，只能用于蓄能。", error);
        Assert.Single(player.Hand);
        Assert.Equal(4, player.Energy);
    }

    [Fact]
    public void RestrictedCard_CanBeCharged_AndAllOutThenComesTooLate()
    {
        var player = NewFighter(BattleSide.Player);
        player.Hand.Add(new CardInstance(CardKind.Action, 12));
        player.Hand.Add(new CardInstance(CardKind.Action, 10));
        player.Energy = 3;
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryCharge(BattleSide.Player, 0, 0, out var error), error);
        Assert.Equal(1, player.Weapons[0].Charge);
        Assert.Equal(12, player.Discard[0].CardId);
        Assert.Equal(10, player.Hand[0].CardId);
        Assert.Equal(0, rules.Flow.TraitActionsThisTurn);
        Assert.False(rules.TryPlay(BattleSide.Player, 0, false, out error));
        Assert.Equal("全力必须是主要阶段的第一个行动。", error);
        Assert.Equal(3, player.Energy);
    }

    [Fact]
    public void TemporaryCard_LeavesHandAtTurnEnd_UnlessTheSubtypeIsRemoved()
    {
        var player = NewFighter(BattleSide.Player);
        var kept = new CardInstance(CardKind.Attack, 8);
        kept.SubtypeOverride = CardSubtypes.None;
        player.Hand.Add(new CardInstance(CardKind.Attack, 8));
        player.Hand.Add(kept);
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        rules.EndMainPhase();
        Assert.Single(player.Hand);
        Assert.Same(kept, player.Hand[0]);
        Assert.Equal(8, player.Removed[0].CardId);
        Assert.Empty(player.Discard);
    }

    [Fact]
    public void Response_OutsideTheWindowIsRejected()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 3;
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.False(rules.TryPlay(BattleSide.Player, 0, true, out var error));
        Assert.Equal("当前没有对应窗口。", error);
        Assert.Single(player.Hand);
        Assert.Equal(3, player.Energy);
    }

    [Fact]
    public void Attack_OutOfRange_KeepsChargeAndLinger()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 2;
        player.Weapons[0].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Attack, 0));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.False(rules.TryAttack(BattleSide.Player, 0, 1, out error));
        Assert.Equal("攻击距离不够。", error);
        Assert.Equal(1, player.Weapons[0].Charge);
        Assert.Single(player.Linger);
        Assert.Equal(20, rules.Enemy.Health);
    }

    [Fact]
    public void Draw_TakesFromTheTopOfTheDeck()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 2;
        player.Deck.Add(new CardInstance(CardKind.Action, 13));
        player.Hand.Add(new CardInstance(CardKind.Action, 3));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Empty(player.Deck);
        Assert.Equal(13, Assert.Single(player.Hand).CardId);
    }

    [Fact]
    public void Range_LingersAndLetsTheNextAttackReach()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Energy = 3;
        player.Weapons[0].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Attack, 3));
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.True(rules.TryAttack(BattleSide.Player, 0, 1, out error), error);
        rules.CloseResponseWindow();
        WeaponChargeTier.Resolve(1, player.Weapons[0].Light, player.Weapons[0].Heavy, out var life, out _, out _);
        Assert.Equal(20 - life, enemy.Health);
        Assert.Empty(player.Linger);
    }

    [Fact]
    public void Cost_ChangesTheNextCardOnly()
    {
        var cards = ActionCards(
            new ActionRow(0, "减费", 0, 0, "7_-1_0"),
            new ActionRow(1, "出牌", 0, 2, "-1"));
        var player = NewFighter(BattleSide.Player);
        player.Energy = 1;
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        var rules = Start(cards, player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out error), error);
        Assert.Equal(0, player.Energy);
        Assert.Equal(0, player.NextCostDelta);
        player.Energy = 1;
        Assert.False(rules.TryPlay(BattleSide.Player, 0, false, out error));
        Assert.Equal("能量不足。", error);
    }

    [Fact]
    public void Discard_WaitsForTheTargetToChoose_ThenFinishesTheCard()
    {
        var cards = ActionCards(
            new ActionRow(0, "逼弃", 0, 0, "9_1_1,1_2_0"),
            new ActionRow(1, "甲", 0, 0, "-1"),
            new ActionRow(2, "乙", 0, 0, "-1"));
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        var first = new CardInstance(CardKind.Action, 1);
        var second = new CardInstance(CardKind.Action, 2);
        enemy.Hand.Add(first);
        enemy.Hand.Add(second);
        var rules = Start(cards, player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.True(rules.WaitingForDiscard);
        Assert.Equal(BattleSide.Enemy, rules.DiscardSide);
        Assert.Equal(1, rules.DiscardCount);
        Assert.Equal(0, player.Energy);
        Assert.Equal(2, enemy.Hand.Count);

        Assert.False(rules.TryPlay(BattleSide.Player, 0, false, out error));
        Assert.Equal("请先选择要弃置的 1 张手牌。", error);
        Assert.Throws<InvalidOperationException>(() => rules.EndMainPhase());
        Assert.False(rules.TryChooseDiscard(BattleSide.Player, new[] { 0 }, out error));
        Assert.False(rules.TryChooseDiscard(BattleSide.Enemy, new[] { 0, 1 }, out error));
        Assert.Equal("需要选择 1 张手牌。", error);

        Assert.True(rules.TryChooseDiscard(BattleSide.Enemy, new[] { 0 }, out error), error);
        Assert.False(rules.WaitingForDiscard);
        Assert.Same(second, Assert.Single(enemy.Hand));
        Assert.Same(first, Assert.Single(enemy.Discard));
        Assert.Equal(2, player.Energy);
    }

    [Fact]
    public void Discard_WithFewerCardsTakesWhatIsLeft_AndEmptyHandSkips()
    {
        var cards = ActionCards(
            new ActionRow(0, "逼弃", 0, 0, "9_2_1"),
            new ActionRow(1, "甲", 0, 0, "-1"));
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        enemy.Hand.Add(new CardInstance(CardKind.Action, 1));
        var rules = Start(cards, player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(1, rules.DiscardCount);
        Assert.True(rules.TryChooseDiscard(BattleSide.Enemy, new[] { 0 }, out error), error);
        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out error), error);
        Assert.False(rules.WaitingForDiscard);
    }

    [Fact]
    public void AllOut_WithDiscardEndsTheMainPhaseOnlyAfterTheChoice()
    {
        var cards = ActionCards(
            new ActionRow(0, "全力逼弃", 2, 0, "9_1_1"),
            new ActionRow(1, "甲", 0, 0, "-1"));
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        enemy.Hand.Add(new CardInstance(CardKind.Action, 1));
        var rules = Start(cards, player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
        Assert.Equal(BattleSide.Player, rules.Flow.ActiveSide);
        Assert.True(rules.TryChooseDiscard(BattleSide.Enemy, new[] { 0 }, out error), error);
        Assert.Equal(BattleSide.Enemy, rules.Flow.ActiveSide);
        Assert.Equal(MainEndReason.AllOut, rules.Flow.MainEndReason);
    }

    [Fact]
    public void DiscardOrNegate_AttackerChooses_AndTheAttackStillHits()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Weapons[0].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Action, 13));
        enemy.Energy = 3;
        enemy.Hand.Add(new CardInstance(CardKind.Action, 1));
        enemy.Weapons.Add(Weapon(0, "w_0"));
        var rules = Start(player, enemy);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        Assert.True(rules.TryPlay(BattleSide.Enemy, 0, true, out error), error);
        Assert.True(rules.WaitingForDiscard);
        Assert.Equal(BattleSide.Player, rules.DiscardSide);
        Assert.Throws<InvalidOperationException>(() => rules.CloseResponseWindow());

        Assert.True(rules.TryChooseDiscard(BattleSide.Player, new[] { 0 }, out error), error);
        rules.CloseResponseWindow();
        Assert.Empty(player.Hand);
        WeaponChargeTier.Resolve(1, player.Weapons[0].Light, player.Weapons[0].Heavy, out var life, out _, out _);
        Assert.Equal(20 - life, enemy.Health);
    }

    [Fact]
    public void RemoveSubtype_KeepsTheAddedTemporaryCardPastTurnEnd()
    {
        var cards = ActionCards(new ActionRow(0, "点化", 0, 0, "5_0_8_1_0,12_3_0"));
        var player = NewFighter(BattleSide.Player);
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        var rules = Start(cards, player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        rules.EndMainPhase();
        var kept = Assert.Single(player.Hand);
        Assert.Equal(8, kept.CardId);
        Assert.Equal(CardSubtypes.None, kept.SubtypeOverride);
        Assert.Empty(player.Removed);
    }

    [Fact]
    public void Negate_WithoutAnAttackIsRejectedBeforePaying()
    {
        var cards = ActionCards(new ActionRow(0, "打消", 0, 1, "11_1"));
        var player = NewFighter(BattleSide.Player);
        player.Energy = 1;
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        var rules = Start(cards, player, NewFighter(BattleSide.Enemy));

        Assert.False(rules.TryPlay(BattleSide.Player, 0, false, out var error));
        Assert.Equal("这张牌只能在攻击进行时使用。", error);
        Assert.Equal(1, player.Energy);
        Assert.Single(player.Hand);
    }

    [Fact]
    public void Negate_InTheWindowCancelsDamage()
    {
        var cards = ActionCards(
            new ActionRow(0, "闪避", 1, 0, "-1"),
            new ActionRow(1, "闪避(对应)", 1, 0, "11_1"));
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Weapons[0].Charge = 1;
        enemy.Hand.Add(new CardInstance(CardKind.Action, 0));
        var rules = Start(cards, player, enemy);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        Assert.True(rules.TryPlay(BattleSide.Enemy, 0, true, out error), error);
        rules.CloseResponseWindow();
        Assert.Equal(20, enemy.Health);
        Assert.Equal(20, enemy.Toughness);
        Assert.Equal(0, player.Weapons[0].Charge);
    }

    [Fact]
    public void Response_SelfStrikeLingersInsteadOfBuffingTheAttacker()
    {
        var cards = ActionCards(
            new ActionRow(0, "架势", 1, 0, "-1"),
            new ActionRow(1, "架势(对应)", 1, 0, "4_1_0_0"));
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Weapons[0].Charge = 1;
        enemy.Hand.Add(new CardInstance(CardKind.Action, 0));
        var rules = Start(cards, player, enemy);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        Assert.True(rules.TryPlay(BattleSide.Enemy, 0, true, out error), error);
        rules.CloseResponseWindow();
        WeaponChargeTier.Resolve(1, player.Weapons[0].Light, player.Weapons[0].Heavy, out var life, out _, out _);
        Assert.Equal(20 - life, enemy.Health);
        Assert.Equal(new[] { 4, 1, 0, 0 }, Assert.Single(enemy.Linger));
        Assert.Empty(player.Linger);
    }

    [Fact]
    public void AttackCard_EnergyLingerStaysWithThePlayer()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 2;
        player.Hand.Add(new CardInstance(CardKind.Attack, 6));
        var enemy = NewFighter(BattleSide.Enemy);
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(new[] { 1, 2, 0 }, Assert.Single(player.Linger));
        Assert.Empty(enemy.Linger);
    }

    [Fact]
    public void Catalog_RejectsAnEffectWithMissingArguments()
    {
        var error = Assert.Throws<InvalidDataException>(() => ActionCards(new ActionRow(0, "缺参", 0, 0, "5_0_1")));
        Assert.Contains("需要 4 个参数", error.Message);
    }

    [Fact]
    public void Combo_AddsOneLifeDamageToWeaponAttacks()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.AddStatus(CardStatuses.Combo);
        player.Weapons[0].Charge = 1;
        var rules = Start(player, enemy);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        rules.CloseResponseWindow();
        WeaponChargeTier.Resolve(1, player.Weapons[0].Light, player.Weapons[0].Heavy, out var life, out var toughness, out _);
        Assert.Equal(20 - life - 1, enemy.Health);
        Assert.Equal(20 - toughness, enemy.Toughness);
    }

    [Fact]
    public void Combo_ComesOnlyFromTheStatus_NotFromTwoActions()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Energy = 6;
        player.Weapons[0].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Action, 3));
        player.Hand.Add(new CardInstance(CardKind.Action, 3));
        player.Hand.Add(new CardInstance(CardKind.Action, 7));
        var rules = Start(player, enemy);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out error), error);
        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out error), error);
        Assert.Equal(8, Assert.Single(player.Hand).CardId);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out error), error);
        rules.CloseResponseWindow();
        WeaponChargeTier.Resolve(1, player.Weapons[0].Light, player.Weapons[0].Heavy, out var life, out _, out _);
        Assert.Equal(20 - life, enemy.Health);
    }

    [Fact]
    public void Combo_ResponseStillUsesTheResponseRow()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.Weapons[0].Charge = 1;
        enemy.AddStatus(CardStatuses.Combo);
        enemy.Energy = 2;
        enemy.Hand.Add(new CardInstance(CardKind.Action, 4));
        enemy.Weapons.Add(new WeaponLoadout(1, "w_1", default, default));
        var rules = Start(player, enemy);

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        Assert.True(rules.TryPlay(BattleSide.Enemy, 0, true, out error), error);
        rules.CloseResponseWindow();
        Assert.Equal(20, enemy.Toughness);
    }

    [Fact]
    public void Swift_GainedAfterTheFirstAttackDoesNotRefundIt()
    {
        var player = NewFighter(BattleSide.Player);
        player.Energy = 4;
        player.Weapons[0].Charge = 1;
        player.Hand.Add(new CardInstance(CardKind.Action, 0));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        rules.CloseResponseWindow();
        Assert.Equal(0, rules.Flow.AttacksRemaining);

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out error), error);
        Assert.True(player.HasStatus(CardStatuses.Swift));
        Assert.True(rules.TryCharge(BattleSide.Player, 0, 0, out error), error);
        Assert.False(rules.TryAttack(BattleSide.Player, 0, 0, out error));
        Assert.Equal("本回合攻击次数已经用完。", error);
    }

    [Fact]
    public void Swift_FirstAttackIsFree_SecondUsesTheCount_ThirdIsRejected()
    {
        var player = NewFighter(BattleSide.Player);
        player.AddStatus(CardStatuses.Swift);
        player.Weapons[0].Charge = 1;
        player.Weapons[1].Charge = 1;
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        rules.CloseResponseWindow();
        Assert.Equal(1, rules.Flow.AttacksRemaining);
        Assert.True(rules.TryAttack(BattleSide.Player, 1, 0, out error), error);
        rules.CloseResponseWindow();
        Assert.Equal(0, rules.Flow.AttacksRemaining);
        player.Weapons[0].Charge = 1;
        Assert.False(rules.TryAttack(BattleSide.Player, 0, 0, out error));
        Assert.Equal("本回合攻击次数已经用完。", error);
    }

    [Fact]
    public void Combo_ClearsForBothSidesAtAnyTurnEnd()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        player.AddStatus(CardStatuses.Combo);
        enemy.AddStatus(CardStatuses.Combo);
        var rules = Start(player, enemy);

        rules.EndMainPhase();
        Assert.False(player.HasStatus(CardStatuses.Combo));
        Assert.False(enemy.HasStatus(CardStatuses.Combo));

        player.AddStatus(CardStatuses.Combo);
        rules.Flow.ResolveShuffle(false);
        Assert.Equal(BattleSide.Enemy, rules.Flow.ActiveSide);
        rules.EndMainPhase();
        Assert.False(player.HasStatus(CardStatuses.Combo));
    }

    [Fact]
    public void Swift_GainedOnTheOpponentsTurnLastsUntilTheHoldersTurnEnd()
    {
        var player = NewFighter(BattleSide.Player);
        var enemy = NewFighter(BattleSide.Enemy);
        var rules = Start(player, enemy);

        rules.EndMainPhase();
        rules.Flow.ResolveShuffle(false);
        Assert.Equal(BattleSide.Enemy, rules.Flow.ActiveSide);
        player.AddStatus(CardStatuses.Swift);
        rules.EndMainPhase();
        Assert.True(player.HasStatus(CardStatuses.Swift));

        rules.Flow.ResolveShuffle(false);
        Assert.Equal(BattleSide.Player, rules.Flow.ActiveSide);
        player.Weapons[0].Charge = 1;
        Assert.True(rules.TryAttack(BattleSide.Player, 0, 0, out var error), error);
        rules.CloseResponseWindow();
        Assert.Equal(1, rules.Flow.AttacksRemaining);
        rules.EndMainPhase();
        Assert.False(player.HasStatus(CardStatuses.Swift));
    }

    static MatchRules Start(FighterState player, FighterState enemy)
    {
        return Start(LoadCards(), player, enemy);
    }

    static MatchRules Start(CardCatalog cards, FighterState player, FighterState enemy)
    {
        var rules = MatchRules.Create(cards, player, enemy);
        rules.Begin(BattleSide.Player);
        rules.Flow.ResolveShuffle(false);
        return rules;
    }

    sealed record ActionRow(int Id, string Name, int Subtype, int Cost, string Effect);

    static CardCatalog ActionCards(params ActionRow[] rows)
    {
        var columns = new[]
        {
            new TableColumn("id", TableFieldType.Int, rows.Length),
            new TableColumn("name", TableFieldType.String, rows.Length),
            new TableColumn("subtype", TableFieldType.Int, rows.Length),
            new TableColumn("cost", TableFieldType.Int, rows.Length),
            new TableColumn("MaxTake", TableFieldType.Int, rows.Length),
            new TableColumn("effect", TableFieldType.IntArray2, rows.Length),
            new TableColumn("owner", TableFieldType.String, rows.Length),
        };
        var ids = new string[rows.Length];
        for (var i = 0; i < rows.Length; i++)
        {
            ids[i] = rows[i].Id.ToString();
            columns[0].Ints[i] = rows[i].Id;
            columns[1].Strings[i] = rows[i].Name;
            columns[2].Ints[i] = rows[i].Subtype;
            columns[3].Ints[i] = rows[i].Cost;
            columns[5].IntArray2s[i] = rows[i].Effect.Split(',').Select(part => part.Split('_').Select(int.Parse).ToArray()).ToArray();
            columns[6].Strings[i] = "null";
        }

        return CardCatalog.Load(Read("AttackCard"), new TableData("ActionCard", columns, ids), Read("BuffCard"));
    }

    static FighterState NewFighter(BattleSide side)
    {
        var fighter = new FighterState(side, "c_0", 4);
        if (side == BattleSide.Player)
        {
            fighter.Weapons.Add(Weapon(0, "w_0"));
            fighter.Weapons.Add(Weapon(1, "w_1"));
        }

        return fighter;
    }

    static WeaponLoadout Weapon(int id, string owner)
    {
        var table = Read("Weapon");
        return new WeaponLoadout(
            id,
            owner,
            WeaponChargeTier.FromArray(table.GetIntArray(id, "charge1")),
            WeaponChargeTier.FromArray(table.GetIntArray(id, "charge2")));
    }

    static CardCatalog LoadCards()
    {
        return CardCatalog.Load(Read("AttackCard"), Read("ActionCard"), Read("BuffCard"));
    }

    static TableData Read(string name)
    {
        var path = Path.GetFullPath(Path.Combine(TableDirectory, name + ".bytes"));
        return TableBinary.Read(File.ReadAllBytes(path));
    }
}
