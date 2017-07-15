using System;
using System.Collections.Generic;
using System.Linq;
using Luegen.Core;

namespace Luegen.BotArena.ConsoleInterface
{
    public class ConsolePlayerController : IPlayerController, IGameListener
    {
        private List<Card> myCards = new List<Card>();

        void IPlayerController.HasFourOf(List<CardRank> hasAllFour)
        {
            myCards.RemoveAll(x => hasAllFour.Contains(x.rank));
        }

        void IPlayerController.ReceivedCards(List<Card> cards)
        {
            myCards.AddRange(cards);
        }

        void IPlayerController.PlayedCards(List<Card> cards)
        {
            myCards.RemoveAll(x => cards.Contains(x));
        }

        List<Card> IPlayerController.SelectCardsAndRank(out CardRank selectedRank)
        {
            WriteHand();
            Console.WriteLine("Which cards do you want to play?");
            Console.WriteLine("Example: diamond king, spade eight, spade king");
            List<Card> selectedCards;
            while (true)
            {
                var input = Console.ReadLine().ToLower();
                selectedCards = parseCards(input);
                if (selectedCards == null || selectedCards.Count == 0)
                {
                    Console.WriteLine("Invalid input!");
                }
                else if (selectedCards.Except(myCards).Any())
                {
                    Console.WriteLine("You can only select cards that are in your hand");
                }
                else
                {
                    break;
                }
            }
            Console.WriteLine("Which rank do you want to announce?");
            Console.WriteLine("Example: king");
            CardRank rank;
            while (true)
            {
                var input = Console.ReadLine().ToLower();
                if (tryParseRank(input, out rank))
                {
                    break;
                }
                Console.WriteLine("Invalid input!");
            }
            selectedRank = rank;
            return selectedCards;
        }


        List<Card> IPlayerController.SelectCardsOrShowdown()
        {
            WriteHand();
            Console.WriteLine("Do you trust the previous player? yes/no");
            while (true)
            {
                var input = Console.ReadLine().ToLower().Trim();
                if (input == "yes")
                {
                    break;
                }
                else if (input == "no")
                {
                    return null;
                }
                Console.WriteLine("Invalid input!");
            }
            Console.WriteLine("Which cards do you want to play?");
            Console.WriteLine("Example: diamond king, spade eight, spade king");
            List<Card> selectedCards;
            while (true)
            {
                var input = Console.ReadLine().ToLower();
                selectedCards = parseCards(input);
                if (selectedCards == null || selectedCards.Count == 0)
                {
                    Console.WriteLine("Invalid input!");
                }
                else if (selectedCards.Except(myCards).Any())
                {
                    Console.WriteLine("You can only select cards that are in your hand");
                }
                else
                {
                    break;
                }
            }
            return selectedCards;
        }

        void WriteHand()
        {
            Console.WriteLine("Cards in your hand:");
            foreach (var card in myCards)
            {
                var suit = Enum.GetName(typeof(CardSuit), card.suit);
                var rank = Enum.GetName(typeof(CardRank), card.rank);
                Console.WriteLine("\t {0} {1}", suit, rank);
            }
        }

        private List<Card> parseCards(string input)
        {
            List<Card> cards = new List<Card>();
            var cardStrings = input.Split(',');
            foreach (var cardString in cardStrings)
            {
                var cardStringSplit = cardString.Trim().Split(' ');
                if (cardStringSplit.Length != 2) return null;
                CardSuit suit;
                CardRank rank;
                if (!Enum.TryParse(cardStringSplit[0], true, out suit))
                {
                    return null;
                }
                if (!Enum.TryParse(cardStringSplit[1], true, out rank))
                {
                    return null;
                }
                cards.Add(new Card(suit, rank));
            }
            return cards;
        }

        private bool tryParseRank(string input, out CardRank rank)
        {
            return Enum.TryParse(input.Trim(), true, out rank);
        }

        void IPlayerController.GameStart(int playerId, int numPlayers)
        {

        }

        void IGameListener.ActiveCardsReveiled(List<Card> reveiledCards)
        {
        }

        void IGameListener.PlayerAnnouncesRank(int playerId, CardRank rank)
        {
        }

        void IGameListener.PlayerHasFourOf(int playerId, List<CardRank> rank)
        {
        }

        void IGameListener.PlayerLooses(int playerId)
        {
        }

        void IGameListener.PlayerMakesTrustDecission(int playerId, TrustDecission decission)
        {
        }

        void IGameListener.PlayerPlaysActiveCard(int playerId, int numCards)
        {
        }

        void IGameListener.PlayerReceivesAllCards(int playerId)
        {
        }

        void IGameListener.PlayerReceivesStartCard(int playerId)
        {
        }

        void IGameListener.PlayerStartTurn(int playerId)
        {
        }

        void IGameListener.ShowdownResult(int previousPlayerId, bool isLie)
        {
        }

    }
}