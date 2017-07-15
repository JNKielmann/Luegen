using System;
using System.Collections.Generic;
using Luegen.Core;

namespace Luegen.BotArena.ConsoleInterface
{
    class ConsoleGameOutput : IGameListener
    {
        private const int sleepTime = 1500;
        public void ActiveCardsReveiled(List<Card> reveiledCards)
        {
            Console.WriteLine("Reveiled Cards: ");
            foreach (var card in reveiledCards)
            {
                Console.WriteLine("\t{0} of {1}", card.rank, card.suit);
            }
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void PlayerAnnouncesRank(int playerId, CardRank rank)
        {
            Console.WriteLine("... and announces {0}", rank.ToString());
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void PlayerHasFourOf(int playerId, List<CardRank> ranks)
        {
            foreach (var rank in ranks)
            {
                Console.WriteLine("Player{0} has four of {1}", playerId + 1, rank.ToString());
            }
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void PlayerLooses(int playerId)
        {
            Console.WriteLine("Player{0} looses", playerId + 1);
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void PlayerMakesTrustDecission(int playerId, TrustDecission decission)
        {
            if (decission == TrustDecission.Trust)
            {
                Console.WriteLine("Player{0} trusts.", playerId + 1);
                System.Threading.Thread.Sleep(sleepTime);
            }
            else
            {
                Console.WriteLine("Player{0} does not trust.", playerId + 1);
                System.Threading.Thread.Sleep(sleepTime);
            }
        }

        public void PlayerPlaysActiveCard(int playerId, int numCards)
        {
            Console.WriteLine("Player{0} plays {1} card(s)", playerId + 1, numCards);
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void PlayerReceivesAllCards(int playerId)
        {
            Console.WriteLine("Player{0} is getting all the cards!", playerId + 1);
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void PlayerReceivesStartCard(int playerId)
        {
        }

        public void PlayerStartTurn(int playerId)
        {
            Console.WriteLine("It is Player{0}'s turn", playerId + 1);
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void ShowdownResult(int previousPlayerId, bool isLie)
        {
            if (isLie)
            {
                Console.WriteLine("Player{0} lied!", previousPlayerId + 1);
                System.Threading.Thread.Sleep(sleepTime);
            }
            else
            {
                Console.WriteLine("Player{0} was telling the truth!", previousPlayerId + 1);
                System.Threading.Thread.Sleep(sleepTime);
            }
        }
    }
}

