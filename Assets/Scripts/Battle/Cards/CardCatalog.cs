using System;
using System.Collections.Generic;
using System.IO;
using CardTables;

// 三张卡牌表。读表时按名字挂上 (连击) 和 (对应) 行，并检查每段效果的参数和目标。
public sealed class CardCatalog
{
    readonly Dictionary<int, CardDefinition>[] tables =
    {
        new Dictionary<int, CardDefinition>(),
        new Dictionary<int, CardDefinition>(),
        new Dictionary<int, CardDefinition>(),
    };

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
        catalog.Add(ReadRows(attackCards, CardKind.Attack));
        catalog.Add(ReadRows(actionCards, CardKind.Action));
        catalog.Add(ReadRows(buffCards, CardKind.Buff));
        return catalog;
    }

    public CardDefinition Get(CardKind kind, int id)
    {
        if (!Table(kind).TryGetValue(id, out var card))
        {
            throw new InvalidDataException(kind + " 表没有 Id " + id + "。");
        }

        return card;
    }

    public bool Contains(CardKind kind, int id)
    {
        return Table(kind).ContainsKey(id);
    }

    // 去掉子类别的牌按 SubtypeOverride 算。
    public int SubtypeOf(CardInstance card)
    {
        return card.SubtypeOverride ?? Get(card.Kind, card.CardId).Subtype;
    }

    public void CopyTo(List<CardDefinition> destination)
    {
        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        for (var i = 0; i < tables.Length; i++)
        {
            destination.AddRange(tables[i].Values);
        }
    }

    Dictionary<int, CardDefinition> Table(CardKind kind)
    {
        var index = (int)kind;
        if (index < 0 || index >= tables.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        return tables[index];
    }

    void Add(List<CardDefinition> rows)
    {
        LinkVariants(rows);
        for (var i = 0; i < rows.Count; i++)
        {
            var card = rows[i];
            Validate(card, card.PlayEffect, "effect");
            Validate(card, card.StartEffect, "starteffect");
            Validate(card, card.OngoingEffect, "effect");
            Validate(card, card.EndEffect, "endeffect");
            var table = Table(card.Kind);
            if (table.ContainsKey(card.Id))
            {
                throw new InvalidDataException("卡牌 Id 重复：" + card.Id + " " + card.Name);
            }

            table.Add(card.Id, card);
        }
    }

    // 攻击和行动牌的 effect 是打出时效果。加成牌的 effect 是展开中，另有 starteffect、endeffect 和 duration。
    static List<CardDefinition> ReadRows(TableData table, CardKind kind)
    {
        var rows = new List<CardDefinition>(table.RowCount);
        for (var row = 0; row < table.RowCount; row++)
        {
            var id = table.GetIntAt(row, "id");
            var name = table.GetStringAt(row, "name");
            var description = ReadDescription(table, row);
            var subtype = table.GetIntAt(row, "subtype");
            var cost = table.GetIntAt(row, "cost");
            var maxTake = table.GetIntAt(row, "MaxTake");
            var owner = table.GetStringAt(row, "owner");
            if (kind == CardKind.Buff)
            {
                rows.Add(new CardDefinition(
                    kind, id, name, description, subtype, cost, maxTake, owner,
                    table.GetIntAt(row, "duration"),
                    null,
                    table.GetIntArray2At(row, "starteffect"),
                    table.GetIntArray2At(row, "effect"),
                    table.GetIntArray2At(row, "endeffect")));
            }
            else
            {
                rows.Add(new CardDefinition(
                    kind, id, name, description, subtype, cost, maxTake, owner,
                    0,
                    table.GetIntArray2At(row, "effect"),
                    null,
                    null,
                    null));
            }
        }

        return rows;
    }

    // 表头沿用了工作表里的拼写 discription。测试用的临时表可以没有这一列。
    static string ReadDescription(TableData table, int row)
    {
        for (var i = 0; i < table.Fields.Count; i++)
        {
            if (table.Fields[i].Name != "discription")
            {
                continue;
            }

            var text = table.GetStringAt(row, "discription");
            if (string.IsNullOrEmpty(text) || text == "null")
            {
                return string.Empty;
            }

            return text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        }

        return string.Empty;
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
            rows[i].Combo = FindVariant(byName, rows[i], "(连击)");
            rows[i].Response = FindVariant(byName, rows[i], "(对应)");
        }
    }

    static CardDefinition FindVariant(Dictionary<string, List<CardDefinition>> byName, CardDefinition card, string suffix)
    {
        if (!byName.TryGetValue(card.Name + suffix, out var list) || list.Count != 1)
        {
            return null;
        }

        list[0].IsAlternate = true;
        return list[0];
    }

    static void Validate(CardDefinition card, int[][] effects, string column)
    {
        for (var i = 0; i < effects.Length; i++)
        {
            var step = effects[i];
            if (CardEffects.IsEmpty(step))
            {
                continue;
            }

            var where = card.Kind + " " + card.Id + " " + card.Name + " 的 " + column + " 第 " + (i + 1) + " 段";
            var length = CardEffects.Length(step[0]);
            if (length < 0)
            {
                throw new InvalidDataException(where + "：未知效果 " + step[0] + "。");
            }

            if (step.Length != length)
            {
                throw new InvalidDataException(where + "：效果 " + step[0] + " 需要 " + (length - 1) + " 个参数，现在是 " + (step.Length - 1) + " 个。");
            }

            var target = step[step.Length - 1];
            if (target != CardEffects.TargetSelf && target != CardEffects.TargetOpponent)
            {
                throw new InvalidDataException(where + "：目标只能是 0 或 1。");
            }
        }
    }
}
