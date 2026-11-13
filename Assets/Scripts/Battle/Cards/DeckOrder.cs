using System;

// 牌库的顺序和抽牌。起手、补牌、洗回弃牌都走这里。
public static class DeckOrder
{
    public static void Shuffle(FighterState fighter, Random random)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var deck = fighter.Deck;
        for (var i = deck.Count - 1; i > 0; i--)
        {
            var swap = random.Next(i + 1);
            var card = deck[i];
            deck[i] = deck[swap];
            deck[swap] = card;
        }
    }

    // 牌库还有牌时不洗。空了才把弃牌洗回，不支付生命。
    public static bool RecycleIfEmpty(FighterState fighter, Random random)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (fighter.Deck.Count > 0)
        {
            return false;
        }

        fighter.Deck.AddRange(fighter.Discard);
        fighter.Discard.Clear();
        Shuffle(fighter, random);
        return true;
    }

    // 从牌库顶抽一张进手牌。牌库空了返回 null。
    public static CardInstance DrawOne(FighterState fighter)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (fighter.Deck.Count == 0)
        {
            return null;
        }

        var card = fighter.Deck[0];
        fighter.Deck.RemoveAt(0);
        fighter.Hand.Add(card);
        return card;
    }
}
