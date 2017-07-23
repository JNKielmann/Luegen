using System;
using System.Collections.Generic;
using System.Linq;

namespace Luegen.Core
{
    enum GameState
    {
        First_Move,
        Later_Move
    }
    public class GameMaster
    {
        private readonly Random random = new Random();
        private List<Player> players;
        private PlayingField field;
        private IGameListener gameListener;

        public GameMaster(IGameListener gameListener)
        {
            players = new List<Player>();
            this.gameListener = gameListener;
        }
        public void AddPlayer(Player player)
        {
            players.Add(player);
        }
        public int StartGame()
        {
            field = new PlayingField();
            for (var playerId = 0; playerId < players.Count; ++playerId)
            {
                players[playerId].GameStart(playerId, players.Count);
            }
            List<Card> deck = CreateCardDeck();
            int nextCardIsForPlayer = 0;
            while (deck.Count > 0)
            {
                Card card = PickRandomCard(deck);
                players[nextCardIsForPlayer].AddCard(card);
                gameListener.PlayerReceivesStartCard(nextCardIsForPlayer);
                nextCardIsForPlayer = (nextCardIsForPlayer + 1) % players.Count;
            }
            Card startCard = new Card(CardSuit.Diamond, CardRank.Seven);
            int currentPlayerIndex = players.FindIndex(player => player.HasCard(startCard));
            Player currentPlayer = players[currentPlayerIndex];
            int previousPlayerIndex = -1;
            Player previousPlayer = null;

            for (var playerId = 0; playerId < players.Count; ++playerId)
            {
                var hasAllFourOfRank = players[playerId].CheckFourCards();
                if (hasAllFourOfRank.Count > 0)
                {
                    gameListener.PlayerHasFourOf(playerId, hasAllFourOfRank);
                }
            }

            GameState gameState = GameState.First_Move;
            while (true)
            {
                try
                {
                    if (players.Where(player => player.HasCards()).Count() == 1)
                    {
                        var isLie = field.AreActiveCardsLie();
                        gameListener.ActiveCardsReveiled(field.GetActiveCards());
                        gameListener.ShowdownResult(previousPlayerIndex, isLie);
                        if (isLie)
                        {
                            AllCardsTo(previousPlayerIndex);
                            previousPlayer = null;
                            previousPlayerIndex = -1;
                            gameState = GameState.First_Move;
                        }
                        else
                        {
                            AllCardsTo(currentPlayerIndex);
                            break;
                        }
                    }
                    if (!currentPlayer.HasCards())
                    {
                        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                        currentPlayer = players[currentPlayerIndex];
                        continue;
                    }

                    gameListener.PlayerStartTurn(currentPlayerIndex);
                    if (gameState == GameState.First_Move)
                    {
                        CardRank selectedRank;
                        var selectedCards = currentPlayer.SelectCardsAndRank(out selectedRank);
                        field.SetActiveCards(selectedCards);
                        gameListener.PlayerPlaysActiveCard(currentPlayerIndex, selectedCards.Count);
                        field.SetRoundRank(selectedRank);
                        gameListener.PlayerAnnouncesRank(currentPlayerIndex, selectedRank);
                        previousPlayer = currentPlayer;
                        previousPlayerIndex = currentPlayerIndex;
                        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                        currentPlayer = players[currentPlayerIndex];
                        gameState = GameState.Later_Move;
                    }
                    else if (gameState == GameState.Later_Move)
                    {
                        var selectedCards = currentPlayer.SelectCardsOrShowdown();
                        if (selectedCards != null)
                        {
                            gameListener.PlayerMakesTrustDecission(currentPlayerIndex, TrustDecission.Trust);
                            field.SetActiveCards(selectedCards);
                            gameListener.PlayerPlaysActiveCard(currentPlayerIndex, selectedCards.Count);
                            previousPlayer = currentPlayer;
                            previousPlayerIndex = currentPlayerIndex;
                            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                            currentPlayer = players[currentPlayerIndex];
                        }
                        else
                        {
                            gameListener.PlayerMakesTrustDecission(currentPlayerIndex, TrustDecission.DoNotTrust);
                            var isLie = field.AreActiveCardsLie();
                            gameListener.ActiveCardsReveiled(field.GetActiveCards());
                            gameListener.ShowdownResult(previousPlayerIndex, isLie);
                            if (isLie)
                            {
                                AllCardsTo(previousPlayerIndex);
                                previousPlayer = null;
                                previousPlayerIndex = -1;
                                gameState = GameState.First_Move;
                            }
                            else
                            {
                                AllCardsTo(currentPlayerIndex);
                                previousPlayer = null;
                                previousPlayerIndex = -1;
                                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                                currentPlayer = players[currentPlayerIndex];
                                gameState = GameState.First_Move;
                            }
                        }
                    }
                }
                catch (PlayerException e)
                {
                    Console.WriteLine("Player Error: " + e.ToString());
                    gameListener.PlayerLooses(e.PlayerId);
                    return e.PlayerId;
                }
            }
            var loosingPlayers = players
                .GroupBy(x => x.GetNegativePoints())
                .OrderByDescending(g => g.Key)
                .First();
            if (loosingPlayers.Count() == 1)
            {
                gameListener.PlayerLooses(players.IndexOf(loosingPlayers.First()));
                return players.IndexOf(loosingPlayers.First());
            }
            // It's a tie
            return -1;
        }

        private void AllCardsTo(int playerIndex)
        {
            field.MoveAllCardsTo(players[playerIndex]);
            gameListener.PlayerReceivesAllCards(playerIndex);
            var hasAllFourOfRank = players[playerIndex].CheckFourCards();
            if (hasAllFourOfRank.Count > 0)
            {
                gameListener.PlayerHasFourOf(playerIndex, hasAllFourOfRank);
            }
        }

        private Card PickRandomCard(List<Card> deck)
        {

            int index = (int)(random.NextDouble() * deck.Count);
            Card pickedCard = deck[index];
            deck.RemoveAt(index);
            return pickedCard;
        }

        private List<Card> CreateCardDeck()
        {
            var from = CardRank.Seven;
            var to = CardRank.Ace;
            var deck = new List<Card>();
            for (CardRank rank = from; rank <= to; ++rank)
            {
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                {
                    deck.Add(new Card(suit, rank));
                }
            }
            return deck;
        }
    }
}

/*
Events:
Player_Receives_Start_Card
Player_Plays_Active_Card (int numCards)
Player_Announces_Rank (CardRank rank)
Player_Trusts
Player_Does_Not_Trust
Active_Cards_Reveiled (List<Card> reveiledCards)
Player_Receives_Cards (List<Card> receivedCards)




 */
