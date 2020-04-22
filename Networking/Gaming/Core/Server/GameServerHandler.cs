using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using SmallProdGame.Networking.Default;
using SmallProdGame.Networking.Gaming.Game;
using SmallProdGame.Networking.Gaming.Messages;
using UnityEngine;
using Random = System.Random;
using SmallProdGame.Utils;
using Unity.Entities;

namespace SmallProdGame.Networking.Gaming.Core.Server {
    public class GameServerHandler : Debugger {
        private Func<Match, ServerGameMaster> _createGameMaster;
        private readonly Queue<MessageQueueEntry> _messageQueue = new Queue<MessageQueueEntry> ();
        private Random _random = new Random ();

        public Dictionary<string, ConnectedClient> clients = new Dictionary<string, ConnectedClient> ();
        public Dictionary<int, Match> matches = new Dictionary<int, Match> ();

        private void OnConnectionFail () { }

        public void SendToMatch (string type, int objectId, object datas, int matchId) {
            if (matches.ContainsKey (matchId)) {
                var m = matches[matchId];
                m.Broadcast ("gamedata", new GlobalGameData (type, objectId, JsonUtility.ToJson (datas)));
            }
        }

        public void OnReceiveData (string data, IPEndPoint ep) {
            _messageQueue.Enqueue (new MessageQueueEntry { Udp = true, Data = data, Ep = ep });
        }

        public void OnReceiveMMData (string data) {
            _messageQueue.Enqueue (new MessageQueueEntry { Udp = false, Data = data });
        }

        private void OnReceiveDataUDP (string data, IPEndPoint ep) {
            var clientExist = clients.ContainsKey (ep.ToString ());
            GlobalMessage response = JsonUtility.FromJson<GlobalMessage> (data);
            if (!clientExist) {
                if (response.type == "join_match") {
                    var req = JsonUtility.FromJson<JoinMatchMessage> (response.data);
                    var matchExist = matches.ContainsKey (req.matchId);
                    if (!matchExist) {
                        Send ("match_notfound", new ErrorMessage (101, "Match not found"), ep);
                        return;
                    }

                    var match = matches[req.matchId];
                    var client = new ConnectedClient (ep, match);
                    var player = new Player (client, Guid.NewGuid ().ToString ("N"), req.key);
                    var res = match.gameMaster.AddPlayer (player, req.key);
                    if (!res) {
                        Send ("match_invalidkey", new ErrorMessage (102, "Match key incorrect"), ep);
                        return;
                    }

                    clients.Add (ep.ToString (), client);

                    match.players.Add (player);
                    Send ("match_joined", new MatchJoinedMessage (player.playerId, match.nbPlayers), ep);
                    Send ("match_joined_infos", match.SendNewPlayerInformations (), ep);
                    match.OnPlayerJoin (player);
                } else {
                    Send ("error", new ErrorMessage (901, "Client not found"), ep);
                }
            } else {
                var client = clients[ep.ToString ()];
                var match = client.match;
                var player = match.players.Find (p => p.client.id == client.id);
                if (match == null) {
                    Send ("error", new ErrorMessage (101, "Match not found"), ep);
                    return;
                }

                if (player == null) {
                    Send ("error", new ErrorMessage (201, "Player not found"), ep);
                    return;
                }

                switch (response.type) {
                    case "ready_match":
                        {
                            if (player.isReady) {
                                Server.Send (new ErrorMessage (202, "Player already ready"), ep);
                                return;
                            }

                            Send ("match_ready", new NullMessage (), ep);
                            match.gameMaster.OnPlayerReady (player);
                            break;
                        }
                    case "gamedata":
                        {
                            var msg = JsonUtility.FromJson<GlobalGameData> (response.data);
                            if (msg.objectId != -1) {
                                if (!match.spawnedObjects.ContainsKey (msg.objectId)) return;
                                if (match.spawnedObjects[msg.objectId].playerKey != player.key) return;
                            }

                            match.gameMaster.OnNewMessage (msg, player);
                            break;
                        }
                    case "leave_match":
                        {
                            clients.Remove (player.client.ep.ToString ());
                            match.players.Remove (player);
                            match.gameMaster.OnPlayerLeave (player);
                            SendMm ("player_leaved", new PlayerLeavedMessage (player.playerId, match.matchId));
                            break;
                        }
                }
            }
        }

        private void OnReceiveMMDataTCP (string data) {
            GlobalMessage msg = JsonUtility.FromJson<GlobalMessage> (data);
            switch (msg.type) {
                case "create_match":
                    {
                        var m = new Match (_createGameMaster, msg.data);
                        World.DefaultGameObjectInjectionWorld = _defaultWorld;
                        matches.Add (m.matchId, m);
                        SendMm ("match_created", new MatchManagementMessage (m.matchId));
                        break;
                    }
                case "delete_match":
                    {
                        var dmmsg = JsonUtility.FromJson<DeleteMatchMessage> (msg.data);
                        var matchExist = matches.ContainsKey (dmmsg.matchId);
                        if (matchExist) {
                            var m = matches[dmmsg.matchId];
                            ManagerComponent.Get ().StartCoroutine (m.Delete ());
                        }

                        SendMm ("match_deleted", new MatchManagementMessage (dmmsg.matchId));
                        break;
                    }
                case "remove_user":
                    {
                        var mmsg = JsonUtility.FromJson<RemoveUserFromMatchMessage> (msg.data);
                        var matchExist = matches.ContainsKey (mmsg.matchId);
                        if (matchExist) {
                            var m = matches[mmsg.matchId];
                            var p = m.players.Find (pa => pa.key == mmsg.userKey);
                            if (p != null) {
                                clients.Remove (p.client.ep.ToString ());
                                m.players.Remove (p);
                                m.gameMaster.OnPlayerLeave (p);
                            }
                        }

                        SendMm ("user_removed", new UserRemovedMessage (mmsg.matchId, mmsg.userKey));
                        break;
                    }
            }
        }

        private void OnMMReady () {
            Client.Send (new GlobalMessage ("connection",
                JsonUtility.ToJson (new ConnectGameServerMessage ("kujyrhtezd852jy7h7r8451d20cj8y45th1bgf2"))));
        }

        public void SendMm (string type, object data) {
            Client.Send (new GlobalMessage (type, JsonUtility.ToJson (data)));
        }

        public void Send (string type, object data, IPEndPoint ep) {
            Server.Send (new GlobalMessage (type, JsonUtility.ToJson (data)), ep);
        }

        public void SendGameData (string type, int objectId, object data, IPEndPoint ep) {
            Server.Send (
                new GlobalMessage ("gamedata",
                    JsonUtility.ToJson (new GlobalGameData (type, objectId, JsonUtility.ToJson (data)))), ep);
        }

        public IEnumerator CoroutineHandler () {
            while (true) {
                yield return new WaitForEndOfFrame ();
                MessageHandler ();
                MatchHandler ();
            }
        }

        public void MatchHandler () {
            foreach (var item in matches) {
                ServerGameMaster gameMaster = item.Value.gameMaster;
                gameMaster.ExecuteQueue ();
            }
        }

        public void MessageHandler () {
            if (_messageQueue.Count > 0) {
                var m = _messageQueue.Dequeue ();
                if (m.Udp)
                    OnReceiveDataUDP (m.Data, m.Ep);
                else
                    OnReceiveMMDataTCP (m.Data);
            }
        }

        public void Stop () {
            Client.Stop ();
            Server.Stop ();
        }

        public struct MessageQueueEntry {
            public bool Udp;
            public string Data;
            public IPEndPoint Ep;
        }

        #region Instance

        private static GameServerHandler _instance;

        private GameServerHandler () { }

        public static GameServerHandler Get () {
            if (_instance == null) _instance = new GameServerHandler ();
            return _instance;
        }

        #endregion

        #region Server Connection

        public MyUdpServer Server;
        public MyTcpClient Client;
        private World _defaultWorld;

        public void Start (ServerDatas gameServerDatas, ServerDatas mmServerDatas, Func<Match, ServerGameMaster> createGameMaster, bool debug = false) {
            this.debug = debug;
            Server = new MyUdpServer ();
            string udpAddress = CliArguments.GetArgument ("--address");
            string udpPort = CliArguments.GetArgument ("--port");
            Server.Start (udpAddress != null ? udpAddress : gameServerDatas.address, udpPort != null ? int.Parse (udpPort) : gameServerDatas.port, debug);
            Client = new MyTcpClient ();
            string mmAddress = CliArguments.GetArgument ("--mmaddress");
            string mmPort = CliArguments.GetArgument ("--mmport");
            Client.Start (mmAddress != null ? mmAddress : mmServerDatas.address, mmPort != null ? int.Parse (mmPort) : mmServerDatas.port, OnMMReady, OnConnectionFail, debug);
            _createGameMaster = createGameMaster;
            _defaultWorld = World.DefaultGameObjectInjectionWorld;
        }

        #endregion
    }
}