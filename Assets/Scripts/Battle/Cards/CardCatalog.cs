using System;
using System.Collections.Generic;
using System.IO;
using CardTables;

public enum CardKind
{
    Attack = 0,
    Action = 1,
    Buff = 2,
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
}

// 一张表行。连击行和对应行挂在初始卡上，不单独进手牌。
public sealed class CardDefinition
{
    public CardDefinition(
        CardKind kind,
        int id,
        string name,
        int subtype,
        int cost,
        int maxTake,
        string owner,
        int duration,
        int[][] playEffect,
        int[][] startEffect,
        int[][] ongoingEffect,
        int[][] endEffect)
    {
        Kind = kind;
        Id = id;
        Name = name ?? string.Empty;
        Subtype = subtype;
        Cost = cost;
        MaxTake = maxTake;
        Owner = string.IsNullOrEmpty(owner) || owner == "null" ? string.Empty : owner;
        Duration = duration;
        PlayEffect = playEffect ?? Array.Empty<int[]>();
        StartEffect = startEffect ?? Array.Empty<int[]>();
        OngoingEffect = ongoingEffect ?? Array.Empty<int[]>();
        EndEffect = endEffect ?? Array.Empty<int[]>();
    }

    public CardKind Kind { get; }
    public int Id { get; }
    public string Name { get; }
    public int Subtype { get; }
    public int Cost { get; }
    public int MaxTake { get; }
    public string Owner { get; }
    public int Duration { get; }
    public int[][] PlayEffect { get; }
    public int[][] StartEffect { get; }
    public int[][] OngoingEffect { get; }
    public int[][] EndEffect { get; }
    public CardDefinition Combo { get; internal set; }
    public CardDefinition Response { get; internal set; }
}

public sealed class CardInstance
{
    public CardInstance(CardKind kind, int cardId)
    {
        Kind = kind;
        CardId = cardId;
        EffectCardId = cardId;
    }

    public CardKind Kind { get; }
    public int CardId { get; }
    public int EffectCardId { get; set; }
    public int Duration { get; set; }
    public int? SubtypeOverride { get; set; }
}

public sealed class CardCatalog
{
    readonly Dictionary<int, CardDefinition> attacks = new Dictionary<int, CardDefinition>();
    readonly Dictionary<int, CardDefinition> actions = new Dictionary<int, CardDefinition>();
    readonly Dictionary<int, CardDefinition> buffs = new Dictionary<int, CardDefinition>();

    CardCatalog()
    {
    }

    public static CardCatalog Load(TableData attackCards, TableData actionCards, TableData buffCards)
    {
        if (attackCards == null || actionCards == null || buffCards == null)
        {
            throw new ArgumentNullException(nameof(attackCards));
        }

        var catalog = new CardCatalog();
        var attackRows = ReadBattleCards(attackCards, CardKind.Attack);
        var actionRows = ReadBattleCards(actionCards, CardKind.Action);
        var buffRows = ReadBuffCards(buffCards);
        LinkVariants(attackRows);
        LinkVariants(actionRows);
        LinkVariants(buffRows);
        Index(catalog.attacks, attackRows);
        Index(catalog.actions, actionRows);
        Index(catalog.buffs, buffRows);
        return catalog;
    }

    public CardDefinition Get(CardKind kind, int id)
    {
        var table = Table(kind);
        if (!table.TryGetValue(id, out var card))
        {
            throw new InvalidDataException(kind + " 表没有 Id " + id + "。");
        }

        return card;
    }

    public bool Contains(CardKind kind, int id)
    {
        return Table(kind).ContainsKey(id);
    }

    Dictionary<int, CardDefinition> Table(CardKind kind)
    {
        if (kind == CardKind.Attack)
        {
            return attacks;
        }

        if (kind == CardKind.Action)
        {
            return actions;
        }

        if (kind == CardKind.Buff)
        {
            return buffs;
        }

        throw new ArgumentOutOfRangeException(nameof(kind));
    }

    static List<CardDefinition> ReadBattleCards(TableData table, CardKind kind)
    {
        var rows = new List<CardDefinition>(table.RowCount);
        for (var row = 0; row < table.RowCount; row++)
        {
            var id = table.GetIntAt(row, "id");
            rows.Add(new CardDefinition(
                kind,
                id,
                table.GetStringAt(row, "name"),
                table.GetIntAt(row, "subtype"),
                table.GetIntAt(row, "cost"),
                table.GetIntAt(row, "MaxTake"),
                table.GetStringAt(row, "owner"),
                0,
                table.GetIntArray2At(row, "effect"),
                Array.Empty<int[]>(),
                Array.Empty<int[]>(),
                Array.Empty<int[]>()));
        }

        return rows;
    }

    static List<CardDefinition> ReadBuffCards(TableData table)
    {
        var rows = new List<CardDefinition>(table.RowCount);
        for (var row = 0; row < table.RowCount; row++)
        {
            var id = table.GetIntAt(row, "id");
            rows.Add(new CardDefinition(
                CardKind.Buff,
                id,
                table.GetStringAt(row, "name"),
                table.GetIntAt(row, "subtype"),
                table.GetIntAt(row, "cost"),
                table.GetIntAt(row, "MaxTake"),
                table.GetStringAt(row, "owner"),
                table.GetIntAt(row, "duration"),
                Array.Empty<int[]>(),
                table.GetIntArray2At(row, "starteffect"),
                table.GetIntArray2At(row, "effect"),
                table.GetIntArray2At(row, "endeffect")));
        }

        return rows;
    }

    static void LinkVariants(List<CardDefinition> rows)
    {
        var byName = new Dictionary<string, List<CardDefinition>>(StringComparer.Ordinal);
        for (var i = 0; i < rows.Count; i++)
        {
            if (!byName.TryGetValue(rows[i].Name, out var list))
            {
                list = new List<CardDefinition>();
                byName.Add(rows[i].Name, list);
            }

            list.Add(rows[i]);
        }

        for (var i = 0; i < rows.Count; i++)
        {
            rows[i].Combo = FindSuffix(byName, rows[i], "(连击)");
            rows[i].Response = FindSuffix(byName, rows[i], "(对应)");
        }
    }

    static CardDefinition FindSuffix(Dictionary<string, List<CardDefinition>> byName, CardDefinition card, string suffix)
    {
        if (!byName.TryGetValue(card.Name + suffix, out var list) || list.Count != 1)
        {
            return null;
        }

        return list[0];
    }

    static void Index(Dictionary<int, CardDefinition> destination, List<CardDefinition> rows)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            if (destination.ContainsKey(rows[i].Id))
            {
                throw new InvalidDataException("卡牌 Id 重复：" + rows[i].Id + " " + rows[i].Name);
            }

            destination.Add(rows[i].Id, rows[i]);
        }
    }
}
