using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class CreateMatchMessage
    {
        public string[] keys;
        public string map;
        public int matchId;
        public string type;
    }
}