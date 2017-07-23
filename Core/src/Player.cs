using System;
using System.Linq;
using System.Collections.Generic;
namespace Luegen.Core
{
    public enum TrustDecission
    {
        Trust,
        DoNotTrust
    }
    public class Player
    {
        private int myId;
        private List<Card> cards;
        private IPlayerController controller;

        private int negativePoints;

        public Player(string name, IPlayerController controller)
        {
            this.controller = controller;
            this.Name = name;
        }

        public string Name { get; }

        public void GameStart(int playerId, int numPlayers)
        {
            myId = playerId;
            cards = new List<Card>();
            negativePoints = 0;
            try
            {
                controller.GameStart(playerId, numPlayers);
            }
            catch (Exception e)
            {
                throw new PlayerException(
                    playerId, e,
                    "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                );
            }
        }

        public void AddCard(Card newCard)
        {
            cards.Add(newCard);
            try
            {
                controller.ReceivedCards(new List<Card> { newCard });
            }
            catch (Exception e)
            {
                throw new PlayerException(
                    myId, e,
                    "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                );
            }
        }

        public void AddCards(List<Card> newCards)
        {
            cards.AddRange(newCards);
            try
            {
                controller.ReceivedCards(newCards);
            }
            catch (Exception e)
            {
                throw new PlayerException(
                    myId, e,
                    "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                );
            }
        }

        public bool HasCard(Card card)
        {
            return cards.Contains(card);
        }

        public List<Card> SelectCardsAndRank(out CardRank selectedRank)
        {
            List<Card> selectedCards;
            try
            {
                selectedCards = controller.SelectCardsAndRank(out selectedRank);
            }
            catch (Exception e)
            {
                throw new PlayerException(
                    myId, e,
                    "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                );
            }
            testCards(selectedCards);
            cards.RemoveAll(x => selectedCards.Contains(x));
            try
            {
                controller.PlayedCards(selectedCards);
            }
            catch (Exception e)
            {
                throw new PlayerException(
                    myId, e,
                    "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                );
            }
            return selectedCards;
        }

        public List<Card> SelectCardsOrShowdown()
        {
            List<Card> selectedCards;
            try
            {
                selectedCards = controller.SelectCardsOrShowdown();
            }
            catch (Exception e)
            {
                throw new PlayerException(
                    myId, e,
                    "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                );
            }
            if (selectedCards != null)
            {
                testCards(selectedCards);
                cards.RemoveAll(x => selectedCards.Contains(x));
                try
                {
                    controller.PlayedCards(selectedCards);
                }
                catch (Exception e)
                {
                    throw new PlayerException(
                        myId, e,
                        "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                    );
                }

            }
            return selectedCards;
        }

        private void testCards(List<Card> testCards)
        {
            if (testCards.Count == 0)
            {
                throw new PlayerException(myId, "Player return empty card selection");
            }
            if (!testCards.All(c => HasCard(c)))
            {
                throw new PlayerException(myId, "Player selected cards that he does not have!");
            }
        }

        public bool HasCards()
        {
            return cards.Count > 0;
        }

        public List<CardRank> CheckFourCards()
        {
            var numCardsWithRank = new int[(int)CardRank.Ace + 1];
            foreach (var card in cards)
            {
                numCardsWithRank[(int)card.rank] += 1;
            }
            var hasAllFourOfRank = new List<CardRank>();
            for (var rank = CardRank.Two; rank <= CardRank.Ace; ++rank)
            {
                if (numCardsWithRank[(int)rank] == 4)
                {
                    hasAllFourOfRank.Add(rank);
                }
            }
            negativePoints += hasAllFourOfRank.Count;
            cards.RemoveAll(x => hasAllFourOfRank.Contains(x.rank));
            try
            {
                controller.HasFourOf(hasAllFourOfRank);
            }
            catch (Exception e)
            {
                throw new PlayerException(
                    myId, e,
                    "Player code cause exception" + e.GetType().ToString() + ": " + e.Message
                );
            }
            return hasAllFourOfRank;
        }

        public int GetNegativePoints()
        {
            return negativePoints;
        }
    }
}