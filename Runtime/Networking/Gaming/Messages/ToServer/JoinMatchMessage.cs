using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class JoinMatchMessage
    {
        public string key;
        public int matchId;

        public JoinMatchMessage(int matchId, string key)
        {
            this.matchId = matchId;
            this.key = key;
        }
    }
}