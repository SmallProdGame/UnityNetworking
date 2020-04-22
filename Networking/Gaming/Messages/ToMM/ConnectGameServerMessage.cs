using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class ConnectGameServerMessage
    {
        public string key;

        public ConnectGameServerMessage(string key)
        {
            this.key = key;
        }
    }
}