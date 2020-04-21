using System;

namespace SmallProdGame.Networking.Matchmaking.Messages
{
    [Serializable]
    public class CreateMatchMessage
    {
        public string map;
        public int maxUser;
        public int minUser;
        public string password;
        public string type;

        public CreateMatchMessage(int maxUser, int minUser, string type, string map, string password)
        {
            this.maxUser = maxUser;
            this.minUser = minUser;
            this.type = type;
            this.map = map;
            this.password = password;
        }
    }
}