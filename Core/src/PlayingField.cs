using System;
using System.Collections.Generic;

namespace Luegen.Core
{
    class PlayingField
    {
        List<Card> activeCards = new List<Card>();
        List<Card> roundCards = new List<Card>();
        CardRank roundRank;

        public void SetActiveCards(List<Card> newActiveCards)
        {
            roundCards.AddRange(activeCards);
            activeCards.Clear();
            activeCards.AddRange(newActiveCards);
        }

        public void MoveAllCardsTo(Player player)
        {
            List<Card> allCards = new List<Card>();
            allCards.AddRange(roundCards);
            allCards.AddRange(activeCards);
            roundCards.Clear();
            activeCards.Clear();
            player.AddCards(allCards);
        }

        public void SetRoundRank(CardRank suit)
        {
            roundRank = suit;
        }

        public bool AreActiveCardsLie()
        {
            return !activeCards.TrueForAll(x => x.rank == roundRank);
        }

        public List<Card> GetActiveCards()
        {
            return activeCards;
        }
    }
}
