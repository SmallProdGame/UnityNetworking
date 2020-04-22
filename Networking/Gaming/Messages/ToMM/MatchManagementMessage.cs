namespace SmallProdGame.Networking.Gaming.Messages
{
    [System.Serializable]
    public class MatchManagementMessage
    {
        public int matchId;

        public MatchManagementMessage(int matchId)
        {
            this.matchId = matchId;
        }
    }
}