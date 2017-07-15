using System;
using System.Collections.Generic;

namespace Luegen.Core
{
    public interface IGameListener
    {
        void PlayerReceivesStartCard(int playerId);

        void PlayerStartTurn(int playerId);

        void PlayerPlaysActiveCard(int playerId, int numCards);

        void PlayerAnnouncesRank(int playerId, CardRank rank);

        void PlayerMakesTrustDecission(int playerId, TrustDecission decission);

        void PlayerReceivesAllCards(int playerId);

        void ActiveCardsReveiled(List<Card> reveiledCards);

        void ShowdownResult(int previousPlayerId, bool isLie);

        void PlayerHasFourOf(int playerId, List<CardRank> rank);

        void PlayerLooses(int playerId);
    }
}