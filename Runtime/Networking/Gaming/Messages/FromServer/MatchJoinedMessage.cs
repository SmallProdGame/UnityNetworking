using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class MatchJoinedMessage
    {
        public int nbPlayers;
        public string playerId;

        public MatchJoinedMessage(string playerId, int nbPlayers)
        {
            this.playerId = playerId;
            this.nbPlayers = nbPlayers;
        }
    }
}