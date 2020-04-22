using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class RemoveUserFromMatchMessage
    {
        public int matchId;
        public string userKey;
    }
}