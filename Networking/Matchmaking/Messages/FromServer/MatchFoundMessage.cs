using System;

namespace SmallProdGame.Networking.Matchmaking.Messages
{
    [Serializable]
    public class MatchFoundMessage
    {
        public int matchId;

        public MatchFoundMessage() { }
    }
}