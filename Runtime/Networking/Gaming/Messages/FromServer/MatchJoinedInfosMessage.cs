using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class MatchJoinedInfosMessage
    {
        public GlobalGameData[] toSpawn;

        public MatchJoinedInfosMessage(GlobalGameData[] toSpawn)
        {
            this.toSpawn = toSpawn;
        }
    }
}