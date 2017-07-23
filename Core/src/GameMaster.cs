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
            var posToPlayerId = Enumerable.Range(0, players.Count).OrderBy(x => random.Next()).ToArray();
            var playerIdToPos = new int[players.Count];
            for(var pos = 0; pos < players.Count; ++pos){
                playerIdToPos[posToPlayerId[pos]] = pos;
            }

            field = new PlayingField();
            for (var playerId = 0; playerId < players.Count; ++playerId)
            {
                players[playerId].GameStart(playerId, posToPlayerId);
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
            int currentPlayerId = players.FindIndex(player => player.HasCard(startCard));
            int currentPlayerPos = playerIdToPos[currentPlayerId];
            Player currentPlayer = players[currentPlayerId];
            int previousPlayerId = -1;
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
                        gameListener.ShowdownResult(previousPlayerId, isLie);
                        if (isLie)
                        {
                            AllCardsTo(previousPlayerId);
                            previousPlayer = null;
                            previousPlayerId = -1;
                            gameState = GameState.First_Move;
                        }
                        else
                        {
                            AllCardsTo(currentPlayerId);
                            break;
                        }
                    }
                    if (!currentPlayer.HasCards())
                    {
                        currentPlayerPos = (currentPlayerPos + 1) % players.Count;
                        currentPlayerId = posToPlayerId[currentPlayerPos];
                        currentPlayer = players[currentPlayerId];
                        continue;
                    }

                    gameListener.PlayerStartTurn(currentPlayerId);
                    if (gameState == GameState.First_Move)
                    {
                        CardRank selectedRank;
                        var selectedCards = currentPlayer.SelectCardsAndRank(out selectedRank);
                        field.SetActiveCards(selectedCards);
                        gameListener.PlayerPlaysActiveCard(currentPlayerId, selectedCards.Count);
                        field.SetRoundRank(selectedRank);
                        gameListener.PlayerAnnouncesRank(currentPlayerId, selectedRank);
                        previousPlayer = currentPlayer;
                        previousPlayerId = currentPlayerId;
                        currentPlayerPos = (currentPlayerPos + 1) % players.Count;
                        currentPlayerId = posToPlayerId[currentPlayerPos];
                        currentPlayer = players[currentPlayerId];
                        gameState = GameState.Later_Move;
                    }
                    else if (gameState == GameState.Later_Move)
                    {
                        var selectedCards = currentPlayer.SelectCardsOrShowdown();
                        if (selectedCards != null)
                        {
                            gameListener.PlayerMakesTrustDecission(currentPlayerId, TrustDecission.Trust);
                            field.SetActiveCards(selectedCards);
                            gameListener.PlayerPlaysActiveCard(currentPlayerId, selectedCards.Count);
                            previousPlayer = currentPlayer;
                            previousPlayerId = currentPlayerId;
                            currentPlayerPos = (currentPlayerPos + 1) % players.Count;
                            currentPlayerId = posToPlayerId[currentPlayerPos];
                            currentPlayer = players[currentPlayerId];
                        }
                        else
                        {
                            gameListener.PlayerMakesTrustDecission(currentPlayerId, TrustDecission.DoNotTrust);
                            var isLie = field.AreActiveCardsLie();
                            gameListener.ActiveCardsReveiled(field.GetActiveCards());
                            gameListener.ShowdownResult(previousPlayerId, isLie);
                            if (isLie)
                            {
                                AllCardsTo(previousPlayerId);
                                previousPlayer = null;
                                previousPlayerId = -1;
                                gameState = GameState.First_Move;
                            }
                            else
                            {
                                AllCardsTo(currentPlayerId);
                                previousPlayer = null;
                                previousPlayerId = -1;
                                currentPlayerPos = (currentPlayerPos + 1) % players.Count;
                                currentPlayerId = posToPlayerId[currentPlayerPos];
                                currentPlayer = players[currentPlayerId];
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
