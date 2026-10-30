using System;

// 起手先打乱牌库，再从牌库顶把手牌补到指定张数。补牌不看手牌上限。
public static class OpeningHand
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

    public static void DrawTo(FighterState fighter, int targetCount)
    {
        if (fighter == null)
        {
            throw new ArgumentNullException(nameof(fighter));
        }

        if (targetCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetCount));
        }

        while (fighter.Hand.Count < targetCount && fighter.Deck.Count > 0)
        {
            var card = fighter.Deck[0];
            fighter.Deck.RemoveAt(0);
            fighter.Hand.Add(card);
        }
    }
}
