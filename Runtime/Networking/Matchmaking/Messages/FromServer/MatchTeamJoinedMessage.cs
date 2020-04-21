using System.Collections.Generic;

namespace SmallProdGame.Networking.Matchmaking.Messages {
    [System.Serializable]
    public class MatchTeamJoinedMessage {
        public List<PlayerLobby> players;
        public string teamId;

        public MatchTeamJoinedMessage () { }
    }

    [System.Serializable]
    public class PlayerLobby {
        public string userId;
        public bool ready;
    }
}