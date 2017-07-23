using System;

namespace Luegen.Core
{
    public class PlayerException : Exception
    {
        public PlayerException(int playerId, string message)
            : base(message)
        {
            PlayerId = playerId;
        }

        public PlayerException(int playerId, Exception innerException, string message)
            : base(message, innerException)
        {
            PlayerId = playerId;
        }

        public int PlayerId { get; private set; }
    }
}