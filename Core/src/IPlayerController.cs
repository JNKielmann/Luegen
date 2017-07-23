using System.Collections.Generic;

namespace Luegen.Core
{
    public interface IPlayerController
    {
        void GameStart(int playerId, int[] positionToPlayerId);

        List<Card> SelectCardsOrShowdown();

        List<Card> SelectCardsAndRank(out CardRank selectedRank);

        void ReceivedCards(List<Card> cards);

        void PlayedCards(List<Card> cards);

        void HasFourOf(List<CardRank> hasAllFour);
    }
}