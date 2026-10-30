using System;

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
    public bool IsAlternate { get; internal set; }
}

// 局内的一张牌。加成牌上场后记住用哪一行结算弃置时，以及剩余读数。
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
