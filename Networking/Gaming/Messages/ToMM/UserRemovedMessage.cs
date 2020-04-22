namespace SmallProdGame.Networking.Gaming.Messages
{
    [System.Serializable]
    public class UserRemovedMessage
    {
        public int matchId;
        public string userKey;

        public UserRemovedMessage(int matchId, string userKey)
        {
            this.matchId = matchId;
            this.userKey = userKey;
        }
    }
}