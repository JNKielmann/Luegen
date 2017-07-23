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
        private int[] posToPlayerId;
        private int[] playerIdToPos;
        private PlayingField field;
        private IGameListener gameListener;
        private int currentPlayerId;
        private int currentPlayerPos;
        private Player currentPlayer;
        private int previousPlayerId;
        private Player previousPlayer;
        private GameState gameState;


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
            ShufflePlayerPositions();
            ForEachPlayer(NotifyGameStarted);
            
            List<Card> deck = CreateCardDeck();
            DealAllCardsFrom(deck);       
            FindStartingPlayer();
            ForEachPlayer(CheckFourCards);            

            while (AtLeastOnePlayerHasCards())
            {
                try
                {
                    if (OnlyOnePlayerHasCards())
                    {
                        DoShowdown();
                        continue;
                    }
                    if (!currentPlayer.HasCards())
                    {
                        SkipPlayer();
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
                        NextPlayersTurn();
                    }
                    else if (gameState == GameState.Later_Move)
                    {
                        var selectedCards = currentPlayer.SelectCardsOrShowdown();
                        if (selectedCards != null)
                        {
                            gameListener.PlayerMakesTrustDecission(currentPlayerId, TrustDecission.Trust);
                            field.SetActiveCards(selectedCards);
                            gameListener.PlayerPlaysActiveCard(currentPlayerId, selectedCards.Count);
                            NextPlayersTurn();
                        }
                        else
                        {
                            gameListener.PlayerMakesTrustDecission(currentPlayerId, TrustDecission.DoNotTrust);
                            DoShowdown();
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
            var loosingPlayers = GetLoosingPlayers();
            if (loosingPlayers.Count() == 1)
            {
                var loosingPlayerId = players.IndexOf(loosingPlayers.First());
                gameListener.PlayerLooses(loosingPlayerId);
                return loosingPlayerId;
            }
            // It's a tie
            return -1;
        }

        private void DoShowdown()
        {
            var isLie = field.AreActiveCardsLie();
            gameListener.ActiveCardsReveiled(field.GetActiveCards());
            gameListener.ShowdownResult(previousPlayerId, isLie);
            if (isLie)
            {
                AllCardsTo(previousPlayerId);
                ThisPlayerStartsNewRound();
            }
            else
            {
                AllCardsTo(currentPlayerId);
                NextPlayerStartsNewRound();
            }
        }

        private List<Player> GetLoosingPlayers()
        {
            return players
                .GroupBy(x => x.GetNegativePoints())
                .OrderByDescending(g => g.Key)
                .First()
                .ToList();
        }

        private void NextPlayerStartsNewRound()
        {
            previousPlayer = null;
            previousPlayerId = -1;

            currentPlayerPos = (currentPlayerPos + 1) % players.Count;
            currentPlayerId = posToPlayerId[currentPlayerPos];
            currentPlayer = players[currentPlayerId];

            gameState = GameState.First_Move;
        }

        private void NextPlayersTurn()
        {
            previousPlayer = currentPlayer;
            previousPlayerId = currentPlayerId;

            currentPlayerPos = (currentPlayerPos + 1) % players.Count;
            currentPlayerId = posToPlayerId[currentPlayerPos];
            currentPlayer = players[currentPlayerId];

            gameState = GameState.Later_Move;
        }

        private void SkipPlayer()
        {
            currentPlayerPos = (currentPlayerPos + 1) % players.Count;
            currentPlayerId = posToPlayerId[currentPlayerPos];
            currentPlayer = players[currentPlayerId];
        }

        private void ThisPlayerStartsNewRound()
        {
            previousPlayer = null;
            previousPlayerId = -1;

            gameState = GameState.First_Move;
        }

        private void FindStartingPlayer()
        {
            Card startCard = new Card(CardSuit.Diamond, CardRank.Seven);
            
            currentPlayerId = players.FindIndex(player => player.HasCard(startCard));
            currentPlayerPos = playerIdToPos[currentPlayerId];
            currentPlayer = players[currentPlayerId];
            previousPlayerId = -1;
            previousPlayer = null;

            gameState = GameState.First_Move;
        }

        private void ShufflePlayerPositions()
        {
            posToPlayerId = Enumerable.Range(0, players.Count).OrderBy(x => random.Next()).ToArray();
            playerIdToPos = new int[players.Count];
            for (var pos = 0; pos < players.Count; ++pos)
            {
                playerIdToPos[posToPlayerId[pos]] = pos;
            }
        }

        private void ForEachPlayer(Action<int> action) 
        {
            for (var playerId = 0; playerId < players.Count; ++playerId)
            {
                action(playerId);
            }
        }

        private void NotifyGameStarted(int playerId) 
        {
            players[playerId].GameStart(playerId, posToPlayerId);
        }

        private void DealAllCardsFrom(List<Card> deck) 
        {
            var nextCardIsForPos = 0;
            while (deck.Count > 0)
            {
                var nextCardIsForPlayerId = posToPlayerId[nextCardIsForPos];
                var card = PickRandomCard(deck);
                players[nextCardIsForPlayerId].AddCard(card);
                gameListener.PlayerReceivesStartCard(nextCardIsForPlayerId);
                nextCardIsForPos = (nextCardIsForPos + 1) % players.Count;
            }
        }

        private bool OnlyOnePlayerHasCards()
        {
            return players.Where(player => player.HasCards()).Count() == 1;
        }

        private bool AtLeastOnePlayerHasCards() {
            return players.Any(player => player.HasCards());
        }

        private void AllCardsTo(int playerId)
        {
            field.MoveAllCardsTo(players[playerId]);
            gameListener.PlayerReceivesAllCards(playerId);
            CheckFourCards(playerId);
        }

        private void CheckFourCards(int playerId)
        {
            var hasAllFourOfRank = players[playerId].CheckFourCards();
            if (hasAllFourOfRank.Count > 0)
            {
                gameListener.PlayerHasFourOf(playerId, hasAllFourOfRank);
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
