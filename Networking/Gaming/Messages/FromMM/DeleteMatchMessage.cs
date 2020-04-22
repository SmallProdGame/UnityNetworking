using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class DeleteMatchMessage
    {
        public int matchId;
    }
}