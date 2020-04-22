namespace SmallProdGame.Networking.Matchmaking.Messages
{
    [System.Serializable]
    public class MatchStartMessage
    {
        public string key;
        public string map;
        public string type;

        public MatchStartMessage()
        {

        }
    }
}