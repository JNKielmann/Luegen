using System;
using System.Linq;
using System.Collections.Generic;
using Luegen.Core;

namespace Luegen.Bots
{
    /*
    Very simple bot that tracks the cards in his hand and plays them with a 
    pretty dumb strategy.
     */
    public class BasicBot : IPlayerController, IGameListener
    {
        private int myId;
        private List<Card> myCards;
        private CardRank roundRank;
        private Random rand;

        /*
         This is called when the game starts. 
         playerId is the id of this player (between 0 and numPlayers - 1)
         numPlayers is the total number of players in the game
         */
        void IPlayerController.GameStart(int playerId, int[] positionToPlayerId)
        {
            myId = playerId;
            myCards = new List<Card>();
            rand = new Random();
        }

        /*
         This is called when you start a new round.
         You have to return a list of cards you want to play and also define 
         which rank you announce (can be a lie or not...)
         */
        List<Card> IPlayerController.SelectCardsAndRank(out CardRank selectedRank)
        {
            // Decide if I want to lie this round
            bool lieThisRound = rand.NextDouble() < 0.5;
            if (lieThisRound)
            {
                // Lie with the rank I have two cards of but only if I have more 
                // than 2 cards on hand
                // If I don't have 2 cards with the same rank, I don't lie
                if (myCards.Count > 2)
                {
                    var twoOfRank = myCards
                        .GroupBy(card => card.rank)
                        .Where(group => group.Count() == 2);
                    if (twoOfRank != null && twoOfRank.Count() > 0)
                    {
                        var rank = RandomElement(twoOfRank).Key;
                        selectedRank = rank;
                        // select up to two cards that are not of my selected rank
                        var selectedCards = myCards
                            .Where(card => card.rank != rank)
                            .OrderBy(x => rand.Next()).Take(2);
                        // return my selection
                        return selectedCards.ToList();
                    }
                }
            }
            // Count how many cards I have of each rank
            var topRank = myCards
                .GroupBy(card => card.rank)
                .OrderByDescending(group => group.Count())
                .First();
            // Select the rank I have the most cards of
            selectedRank = topRank.Key;
            // Play one card of the selected rank
            return new List<Card> { topRank.First() };
        }
        /*
         This is called when it's your turn during a round
         You can either trust the previous player by returning a list of cards
         that you will play or you return null and don't trust. In that case 
         there will be a showdown. If the previous player actually lied, he 
         gets all cards on the table. If he told the truth you get all cards.
         */
        List<Card> IPlayerController.SelectCardsOrShowdown()
        {
            // Decide if I want to lie
            if (rand.NextDouble() < 0.2)
            {
                // Play a random card (probably a lie)
                var randomCard = myCards[(int)(rand.NextDouble() * myCards.Count)];
                return new List<Card> { randomCard };
            }
            else
            {
                // Play a card with the correct rank
                var cardWithRoundRank = myCards.Find(card => card.rank == roundRank);
                if (cardWithRoundRank != null)
                {
                    return new List<Card> { cardWithRoundRank };
                }
                // If I don't have a card with the correct rank, I dont trust my
                // previous player and go for a showdown
                return null;
            }
        }

        /*
         This is called when you receive cards. This can happen because of 3 things:
         1. It's the start of the game and the cards are dealed out.
         2. You lied in your last turn and the player after you did not trust.
         3. You did not trust the previous player but he told the truth.
         */
        void IPlayerController.ReceivedCards(List<Card> cards)
        {
            myCards.AddRange(cards);
        }

        /*
         This is called when you played cards. 
         This happens if it is your turn and you play cards.
         */
        void IPlayerController.PlayedCards(List<Card> cards)
        {
            myCards.RemoveAll(x => cards.Contains(x));
        }

        /*
         This is called when you have 4 cards of the same rank. These 4 cards 
         will be removed from play and you receive a negative point. This method
         will only be called after a call to ReceivedCards.
         */
        void IPlayerController.HasFourOf(List<CardRank> hasAllFour)
        {
            myCards.RemoveAll(x => hasAllFour.Contains(x.rank));
        }

        /*
         Game event: Active cards (cards played by last player) have been reveiled.
         */
        void IGameListener.ActiveCardsReveiled(List<Card> reveiledCards)
        {
        }

        /*
         Game event: Player who starts the round announced rank for this round.
         */
        void IGameListener.PlayerAnnouncesRank(int playerId, CardRank rank)
        {
            roundRank = rank;
        }

        /*
         Game event: A player has 4 cards of the same rank.
         */
        void IGameListener.PlayerHasFourOf(int playerId, List<CardRank> rank)
        {
        }
        /*
         Game event: A player lost the game.
         */
        void IGameListener.PlayerLooses(int playerId)
        {
        }
        /*
         Game event: A player decided wether to trust or not to trust the 
         previous player.
         */
        void IGameListener.PlayerMakesTrustDecission(int playerId, TrustDecission decission)
        {
        }
        /*
         Game event: A player played one ore more cards that are now the 
         active cards.
         */
        void IGameListener.PlayerPlaysActiveCard(int playerId, int numCards)
        {
        }
        /*
         Game event: A player receives all cards on the table.
         */
        void IGameListener.PlayerReceivesAllCards(int playerId)
        {
        }
        /*
         Game event: A player gets dealed a card at the beginning of the game.
         */
        void IGameListener.PlayerReceivesStartCard(int playerId)
        {
        }
        /*
         Game event: A player starts his turn.
         */
        void IGameListener.PlayerStartTurn(int playerId)
        {
        }
        /*
         Game event: Result of a showdown.
         */
        void IGameListener.ShowdownResult(int previousPlayerId, bool isLie)
        {
        }

        /*
         Helper that returns a random element from the enumerable.
         */
        private T RandomElement<T>(IEnumerable<T> enumerable)
        {
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

    }
}