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

        public int PlayerId { get; private set; }
    }
}