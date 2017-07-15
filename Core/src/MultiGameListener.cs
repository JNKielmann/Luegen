using System;
using System.Collections.Generic;

namespace Luegen.Core
{
    public class MultiGameListener : IGameListener
    {
        List<IGameListener> listeners = new List<IGameListener>();

        public void AddGameListener(IGameListener listener)
        {
            listeners.Add(listener);
        }

        public void ActiveCardsReveiled(List<Card> reveiledCards)
        {
            listeners.ForEach(l => l.ActiveCardsReveiled(reveiledCards));
        }

        public void PlayerAnnouncesRank(int playerId, CardRank rank)
        {
            listeners.ForEach(l => l.PlayerAnnouncesRank(playerId, rank));
        }

        public void PlayerHasFourOf(int playerId, List<CardRank> rank)
        {
            listeners.ForEach(l => l.PlayerHasFourOf(playerId, rank));
        }

        public void PlayerLooses(int playerId)
        {
            listeners.ForEach(l => l.PlayerLooses(playerId));
        }

        public void PlayerMakesTrustDecission(int playerId, TrustDecission decission)
        {
            listeners.ForEach(l => l.PlayerMakesTrustDecission(playerId, decission));
        }

        public void PlayerPlaysActiveCard(int playerId, int numCards)
        {
            listeners.ForEach(l => l.PlayerPlaysActiveCard(playerId, numCards));
        }

        public void PlayerReceivesAllCards(int playerId)
        {
            listeners.ForEach(l => l.PlayerReceivesAllCards(playerId));
        }

        public void PlayerReceivesStartCard(int playerId)
        {
            listeners.ForEach(l => l.PlayerReceivesStartCard(playerId));
        }

        public void PlayerStartTurn(int playerId)
        {
            listeners.ForEach(l => l.PlayerStartTurn(playerId));
        }

        public void ShowdownResult(int previousPlayerId, bool isLie)
        {
            listeners.ForEach(l => l.ShowdownResult(previousPlayerId, isLie));
        }
    }
}