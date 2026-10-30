using System;
using System.Collections.Generic;
using System.Globalization;

// 组卡：1 名角色，武器固定为拳套和大剑，两把都进场。
// 名字末尾带括号备注的牌不进牌库，例如 (对应)、(连击)，全角括号也一样。
// MaxTake 为 0 的牌是局内才加入的，不进初始牌库。
// 一张基础牌进组的张数用它的 MaxTake。角色和武器表里没有另一列张数。
public static class DeckBuilder
{
    public const int GlovesId = 0;
    public const int GreatswordId = 1;
    public static string CharacterOwner(int characterId)
    {
        return "c_" + characterId.ToString(CultureInfo.InvariantCulture);
    }

    public static string WeaponOwner(int weaponId)
    {
        return "w_" + weaponId.ToString(CultureInfo.InvariantCulture);
    }

    public static List<CardInstance> Collect(CardCatalog cards, string owner)
    {
        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        var rows = new List<CardDefinition>();
        cards.CopyTo(rows);
        rows.Sort(CompareCards);
        var deck = new List<CardInstance>();
        for (var i = 0; i < rows.Count; i++)
        {
            var card = rows[i];
            if (card.IsAlternate || HasNameRemark(card.Name) || card.MaxTake <= 0 || card.Owner != owner)
            {
                continue;
            }

            for (var copy = 0; copy < card.MaxTake; copy++)
            {
                deck.Add(new CardInstance(card.Kind, card.Id));
            }
        }

        return deck;
    }

    public static List<string> Summarize(CardCatalog cards, IReadOnlyList<CardInstance> deck)
    {
        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        if (deck == null)
        {
            throw new ArgumentNullException(nameof(deck));
        }

        var order = new List<string>();
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < deck.Count; i++)
        {
            var name = cards.Get(deck[i].Kind, deck[i].CardId).Name;
            if (!counts.ContainsKey(name))
            {
                order.Add(name);
                counts.Add(name, 0);
            }

            counts[name]++;
        }

        var lines = new List<string>(order.Count);
        for (var i = 0; i < order.Count; i++)
        {
            lines.Add(order[i] + " ×" + counts[order[i]].ToString(CultureInfo.InvariantCulture));
        }

        return lines;
    }

    public static DeckBuildResult Build(CardCatalog cards, int characterId)
    {
        return Build(cards, characterId, GlovesId, GreatswordId);
    }

    public static DeckBuildResult Build(CardCatalog cards, int characterId, int firstWeapon, int secondWeapon)
    {
        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        if (firstWeapon == secondWeapon)
        {
            throw new ArgumentException("两把武器必须不同。");
        }

        var deck = Collect(cards, CharacterOwner(characterId));
        deck.AddRange(Collect(cards, WeaponOwner(firstWeapon)));
        deck.AddRange(Collect(cards, WeaponOwner(secondWeapon)));
        return new DeckBuildResult(characterId, firstWeapon, secondWeapon, deck);
    }

    public static FighterState CreatePlayer(DeckBuildResult result, int handLimit, WeaponLoadout firstWeapon, WeaponLoadout secondWeapon)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var fighter = new FighterState(BattleSide.Player, CharacterOwner(result.CharacterId), handLimit);
        fighter.Weapons.Add(firstWeapon ?? throw new ArgumentNullException(nameof(firstWeapon)));
        fighter.Weapons.Add(secondWeapon ?? throw new ArgumentNullException(nameof(secondWeapon)));
        fighter.Deck.AddRange(result.Deck);
        return fighter;
    }

    public static bool HasNameRemark(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return EndsWithRemark(name, '(', ')') || EndsWithRemark(name, '（', '）');
    }

    static bool EndsWithRemark(string name, char open, char close)
    {
        if (name[name.Length - 1] != close)
        {
            return false;
        }

        var openAt = name.LastIndexOf(open);
        return openAt > 0 && openAt < name.Length - 1;
    }

    static int CompareCards(CardDefinition left, CardDefinition right)
    {
        var kind = left.Kind.CompareTo(right.Kind);
        if (kind != 0)
        {
            return kind;
        }

        return left.Id.CompareTo(right.Id);
    }
}

public sealed class DeckBuildResult
{
    public DeckBuildResult(int characterId, int weaponId1, int weaponId2, List<CardInstance> deck)
    {
        CharacterId = characterId;
        WeaponId1 = weaponId1;
        WeaponId2 = weaponId2;
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
    }

    public int CharacterId { get; }
    public int WeaponId1 { get; }
    public int WeaponId2 { get; }
    public List<CardInstance> Deck { get; }
}
