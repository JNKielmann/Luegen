namespace Luegen.Core
{
    public class Card
    {
        public readonly CardSuit suit;
        public readonly CardRank rank;

        public Card(CardSuit suit, CardRank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Card other = obj as Card;
            if (other == null) return false;
            return other.suit == suit && other.rank == rank;
        }

        public override int GetHashCode()
        {
            return (int)suit ^ (int)rank;
        }
    }

    public enum CardRank
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    public enum CardSuit
    {
        Diamond,
        Heart,
        Club,
        Spade
    }
}