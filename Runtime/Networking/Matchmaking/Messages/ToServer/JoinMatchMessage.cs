namespace SmallProdGame.Networking.Matchmaking.Messages
{
    [System.Serializable]
    public class JoinMatchMessage
    {
        public int matchId;
        public string password;

        public JoinMatchMessage(int matchId, string password)
        {
            this.matchId = matchId;
            this.password = password;
        }
    }
}