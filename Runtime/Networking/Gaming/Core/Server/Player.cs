namespace SmallProdGame.Networking.Gaming.Core.Server
{
    public class Player
    {
        public ConnectedClient client;
        public bool isReady;
        public string key;
        public string playerId;

        public Player(ConnectedClient client, string key, string playerId)
        {
            this.client = client;
            this.key = key;
            this.playerId = playerId;
            this.isReady = false;
        }
    }
}