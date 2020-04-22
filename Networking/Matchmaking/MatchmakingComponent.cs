using System.Collections.Generic;
using SmallProdGame.Networking.Default;
using SmallProdGame.Networking.Gaming.Game;
using SmallProdGame.Networking.Matchmaking.Messages;
using UnityEngine;

namespace SmallProdGame.Networking.Matchmaking {
    public class MatchmakingComponent : MonoBehaviour {
        // Events
        public delegate void OnMmConnectedA ();

        public delegate void OnMmMatchCreatedA (int matchId);

        public delegate void OnMmMatchFoundA (int matchId);

        public delegate void OnMmMatchJoinedA (int matchId, string userId, string teamId);

        public delegate void OnMmMatchNotFoundA ();

        public delegate void OnMmMatchPlayerLeavedA (string userId);

        public delegate void OnMmMatchPlayerReadyA (string userId);

        public delegate void OnMmMatchReadyA ();

        public delegate void OnMmMatchStartA (string key, string map, string type);

        public delegate void OnMmMatchTeamJoinedA (List<PlayerLobby> players, string teamId);

        public delegate void OnMmMatchTeamLeavedA (string teamId);

        public delegate void OnMmMatchWrongPasswordA ();

        private ManagerComponent _managerComponent;

        private int _matchId = -1;
        private string _userId;

        public ClientHandler clientHandler;

        public bool debug;
        public string defaultMap;
        public string defaultType = "normal";
        public int maxPlayer;

        [Header ("Default match infos")] public int minPlayer;

        [SerializeField] public ServerDatas serverDatas;

        public event OnMmConnectedA OnMmConnected;
        public event OnMmMatchCreatedA OnMmMatchCreated;
        public event OnMmMatchFoundA OnMmMatchFound;
        public event OnMmMatchNotFoundA OnMmMatchNotFound;
        public event OnMmMatchJoinedA OnMmMatchJoined;
        public event OnMmMatchTeamJoinedA OnMmMatchTeamJoined;
        public event OnMmMatchWrongPasswordA OnMmMatchWrongPassword;
        public event OnMmMatchReadyA OnMmMatchReady;
        public event OnMmMatchPlayerReadyA OnMmMatchPlayerReady;
        public event OnMmMatchPlayerLeavedA OnMmMatchPlayerLeaved;
        public event OnMmMatchTeamLeavedA OnMmMatchTeamLeaved;
        public event OnMmMatchStartA OnMmMatchStart;

        private void Start () { }

        public virtual void StartMatchMaking () {
            if (NetworkInfos.IsServer) return;
            Print ("Starting matchmaking....");
            _managerComponent = ManagerComponent.Get ();
            clientHandler = ClientHandler.Get ();
            clientHandler.Start (serverDatas, this, debug);
            StartCoroutine (clientHandler.client.GetDatas (clientHandler.OnReceiveData));
        }

        public virtual void FindMatch () {
            if (NetworkInfos.IsServer) return;
            Print ("Finding match....");
            clientHandler.Send ("find_match", new FindMatchMessage (maxPlayer, minPlayer, defaultMap, defaultType));
        }

        public virtual void CreateMatch () {
            if (NetworkInfos.IsServer) return;
            Print ("Creating match....");
            clientHandler.Send ("create_match",
                new CreateMatchMessage (maxPlayer, minPlayer, defaultType, defaultMap, ""));
        }

        public virtual void JoinMatch () {
            if (NetworkInfos.IsServer) return;
            Print ("Joining match lobby....");
            clientHandler.Send ("join_match", new JoinMatchMessage (_matchId, ""));
        }

        public virtual void ReadyMatchLobby () {
            if (NetworkInfos.IsServer) return;
            Print ("Ready for the match....");
            clientHandler.Send ("ready_match", new NullMessage ());
        }

        public virtual void CancelFindMatch () {
            if (NetworkInfos.IsServer) return;
            Print ("Cancel find match");
            clientHandler.Send ("cancel_find_match", new NullMessage ());
            _matchId = -1;
        }

        public virtual void RefuseMatch () {
            if (NetworkInfos.IsServer) return;
            if (_matchId == -1) return;
            Print ("Refuse match");
            clientHandler.Send ("refuse_match", new NullMessage ());
            _matchId = -1;
        }

        public virtual void LeaveMatch () {
            if (NetworkInfos.IsServer) return;
            if (_matchId == -1) return;
            Print ("Leave match");
            clientHandler.Send ("leave_match", new NullMessage ());
            _matchId = -1;
        }

        [HideInInspector]
        public virtual void OnConnected () {
            if (NetworkInfos.IsServer) return;
            OnMmConnected?.Invoke ();
            Print ("Connected to the matchmaking server!");
        }

        [HideInInspector]
        public virtual void OnMatchCreated (string data) {
            if (NetworkInfos.IsServer) return;
            MatchCreatedMessage msg = JsonUtility.FromJson<MatchCreatedMessage> (data);
            Print ("Match created!");
            OnMmMatchCreated?.Invoke (msg.matchId);
            _matchId = msg.matchId;
        }

        [HideInInspector]
        public virtual void OnMatchFound (string data) {
            if (NetworkInfos.IsServer) return;
            var mfmsg = JsonUtility.FromJson<MatchFoundMessage> (data);
            Print ("Match found!");
            OnMmMatchFound?.Invoke (mfmsg.matchId);
            _matchId = mfmsg.matchId;
        }

        [HideInInspector]
        public virtual void OnMatchNotFound () {
            if (NetworkInfos.IsServer) return;
            OnMmMatchNotFound?.Invoke ();
            Print ("Match not found!");
        }

        [HideInInspector]
        public virtual void OnMatchWrongPassword () {
            if (NetworkInfos.IsServer) return;
            OnMmMatchWrongPassword?.Invoke ();
            Print ("Wrong password!");
        }

        [HideInInspector]
        public virtual void OnMatchJoined (string data) {
            if (NetworkInfos.IsServer) return;
            MatchJoinedMessage mmsg = JsonUtility.FromJson<MatchJoinedMessage> (data);
            OnMmMatchJoined?.Invoke (mmsg.matchId, mmsg.userId, mmsg.teamId);
            Print ("Successfully joined match!");
            _matchId = mmsg.matchId;
            _userId = mmsg.userId;
        }

        [HideInInspector]
        public virtual void OnMatchTeamJoined (string data) {
            if (NetworkInfos.IsServer) return;
            MatchTeamJoinedMessage mmsg = JsonUtility.FromJson<MatchTeamJoinedMessage> (data);
            OnMmMatchTeamJoined?.Invoke (mmsg.players, mmsg.teamId);
            Print ("Team " + mmsg.teamId + " has joined the looby!");
        }

        [HideInInspector]
        public virtual void OnMatchReady (string data) {
            if (NetworkInfos.IsServer) return;
            OnMmMatchReady?.Invoke ();
            Print ("Ready!");
        }

        [HideInInspector]
        public virtual void OnMatchPlayerReady (string data) {
            if (NetworkInfos.IsServer) return;
            MatchPlayerReadyMessage msg = JsonUtility.FromJson<MatchPlayerReadyMessage> (data);
            OnMmMatchPlayerReady?.Invoke (msg.userId);
            Print (msg.userId + " is ready");
        }

        [HideInInspector]
        public virtual void OnMatchTeamLeaved (string data) {
            if (NetworkInfos.IsServer) return;
            MatchTeamLeavedMessage msg = JsonUtility.FromJson<MatchTeamLeavedMessage> (data);
            OnMmMatchTeamLeaved?.Invoke (msg.teamId);
            Print ("Team " + msg.teamId + " leaved");
        }

        [HideInInspector]
        public virtual void OnMatchPlayerLeaved (string data) {
            if (NetworkInfos.IsServer) return;
            MatchPlayerLeavedMessage mmsg = JsonUtility.FromJson<MatchPlayerLeavedMessage> (data);
            OnMmMatchPlayerLeaved?.Invoke (mmsg.userId);
            Print ("Player " + mmsg.userId + " leaved the room!");
        }

        [HideInInspector]
        public virtual void OnMatchStart (string data) {
            if (NetworkInfos.IsServer) return;
            MatchStartMessage mmsg = JsonUtility.FromJson<MatchStartMessage> (data);
            OnMmMatchStart?.Invoke (mmsg.key, mmsg.map, mmsg.type);
        }

        private void Print (string msg) {
            if (debug) Debug.Log (msg);
        }

        private void OnApplicationQuit () {
            if (NetworkInfos.IsServer) return;
            clientHandler.client.Stop ();
        }
    }
}