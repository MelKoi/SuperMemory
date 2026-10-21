using CardTables;
using Xunit;

public class CardRulesTests
{
    static readonly string TableDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Assets", "StreamingAssets", "Tables");

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
        Assert.Equal(2, player.Energy);
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
        player.SetToughness(10);
        player.Hand.Add(new CardInstance(CardKind.Action, 1));
        var rules = Start(player, NewFighter(BattleSide.Enemy));

        Assert.True(rules.TryPlay(BattleSide.Player, 0, false, out var error), error);
        Assert.Equal(0, player.Energy);
        Assert.Equal(12, player.Toughness);
        Assert.Equal(MatchTiming.Main, rules.Flow.Timing);
    }

    [Fact]
    public void BuffCard_OngoingSettlesAtTurnStart_BeforeDurationTicks()
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
        Assert.Equal(13, player.Hand[0].CardId);

        player.Field[0].Duration = 1;
        rules.Flow.ResolveShuffle(false);
        rules.EndMainPhase();
        Assert.Empty(player.Field);
        Assert.Equal(0, player.Discard[0].CardId);
        Assert.Equal(2, player.Hand.Count);
    }

    [Fact]
    public void BuffCard_ComboRowAddsTheExtraCard()
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
        Assert.All(player.Hand, card => Assert.Equal(13, card.CardId));
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
        Assert.True(player.HasStatus(CardStatuses.Combo));
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

    static MatchRules Start(FighterState player, FighterState enemy)
    {
        var rules = MatchRules.Create(LoadCards(), player, enemy);
        rules.Begin(BattleSide.Player);
        rules.Flow.ResolveShuffle(false);
        return rules;
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
