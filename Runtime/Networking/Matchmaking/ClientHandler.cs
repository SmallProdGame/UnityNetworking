using System;
using SmallProdGame.Networking.Default;
using UnityEngine;
using EventHandler = SmallProdGame.Utils.EventHandler;

namespace SmallProdGame.Networking.Matchmaking {
    public class ClientHandler : EventHandler {
        public void Send (string type, object data) {
            client.Send (new GlobalMessage (type, JsonUtility.ToJson (data)));
        }

        private void OnReady () {
            Emit ("connectionsuccess", null);
            _matchmakingComponent.OnConnected ();
        }

        public void OnReceiveData (string data) {
            try {
                if (string.IsNullOrEmpty (data)) return;
                var msg = JsonUtility.FromJson<GlobalMessage> (data);
                switch (msg.type) {
                    case "match_created":
                        {
                            _matchmakingComponent.OnMatchCreated (msg.data);
                            break;
                        }
                    case "match_found":
                        {
                            _matchmakingComponent.OnMatchFound (msg.data);
                            break;
                        }
                    case "match_joined":
                        {
                            _matchmakingComponent.OnMatchJoined (msg.data);
                            break;
                        }
                    case "match_notfound":
                        {
                            _matchmakingComponent.OnMatchNotFound ();
                            break;
                        }
                    case "match_wrongpassword":
                        {
                            _matchmakingComponent.OnMatchWrongPassword ();
                            break;
                        }
                    case "match_team_joined":
                        {
                            _matchmakingComponent.OnMatchTeamJoined (msg.data);
                            break;
                        }
                    case "match_ready":
                        {
                            _matchmakingComponent.OnMatchReady (msg.data);
                            break;
                        }
                    case "match_player_ready":
                        {
                            _matchmakingComponent.OnMatchPlayerReady (msg.data);
                            break;
                        }
                    case "match_team_leaved":
                        {
                            _matchmakingComponent.OnMatchTeamLeaved (msg.data);
                            break;
                        }
                    case "match_player_leaved":
                        {
                            _matchmakingComponent.OnMatchPlayerLeaved (msg.data);
                            break;
                        }
                    case "match_start":
                        {
                            _matchmakingComponent.OnMatchStart (msg.data);
                            break;
                        }
                    default:
                        {
                            Emit (msg.type, msg.data);
                            break;
                        }
                }
            } catch (UnityException) {
                // DO NOTHING
            }
        }

        private void OnConnectionFail () {
            Emit ("connectionfail", null);
        }

        protected override void InitEvents () {
            // DO NOTHING
        }

        public new void On (string name, Func<object, object> func) {
            base.On (name, func);
        }

        #region Instance

        private static SmallProdGame.Networking.Matchmaking.ClientHandler _instance;

        private ClientHandler () {
            // Nothing here for now
        }

        public static ClientHandler Get () {
            if (_instance == null) _instance = new ClientHandler ();
            return _instance;
        }

        #endregion

        #region Server Connection

        public MyTcpClient client;
        private MatchmakingComponent _matchmakingComponent;

        public void Start (ServerDatas serverDatas, MatchmakingComponent matchmakingComponent, bool debug = false) {
            _matchmakingComponent = matchmakingComponent;
            client = new MyTcpClient ();
            client.Start (serverDatas.address, serverDatas.port, OnReady, OnConnectionFail, debug);
        }

        #endregion
    }
}