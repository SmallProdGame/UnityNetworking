using System;

namespace SmallProdGame.Networking.Matchmaking.Messages
{
    [Serializable]
    public class FindMatchMessage
    {
        public string map;
        public int maxUser;
        public int minUser;
        public string type;

        public FindMatchMessage(int maxUser, int minUser, string map, string type)
        {
            this.maxUser = maxUser;
            this.minUser = minUser;
            this.map = map;
            this.type = type;
        }
    }
}