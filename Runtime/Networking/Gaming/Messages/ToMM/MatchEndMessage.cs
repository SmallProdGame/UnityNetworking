namespace SmallProdGame.Networking.Gaming.Messages
{
    [System.Serializable]
    public class MatchEndMessage
    {
        public int matchId;

        public MatchEndMessage(int matchId)
        {
            this.matchId = matchId;
        }
    }
}