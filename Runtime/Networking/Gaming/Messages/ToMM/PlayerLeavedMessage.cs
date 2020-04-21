using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class PlayerLeavedMessage
    {
        public string key;
        public int matchId;

        public PlayerLeavedMessage(string key, int matchId)
        {
            this.key = key;
            this.matchId = matchId;
        }
    }
}