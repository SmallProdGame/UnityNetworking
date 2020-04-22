namespace SmallProdGame.Networking.Matchmaking.Messages
{
    [System.Serializable]
    public class MatchJoinedMessage
    {
        public int matchId;
        public string userId;
        public string teamId;

        public MatchJoinedMessage() { }
    }
}