using System;
using SmallProdGame.Networking.Default;
using SmallProdGame.Networking.Gaming.Messages;
using SmallProdGame.Utils;
using Unity.Entities;
using UnityEngine;

namespace SmallProdGame.Networking.Gaming.Core.Client {
    public class ClientHandler : Debugger {
        private Func<ClientGameMaster> _createGameMaster;

        private bool _ready = true;
        private World _world;

        public bool GetReady () {
            return _ready;
        }

        public void OnReceiveData (string data) {
            _ready = false;
            GlobalMessage msg = JsonUtility.FromJson<GlobalMessage> (data);
            switch (msg.type) {
                case "matchjoinedinfos":
                    {
                        _gameMaster.Reinit ();
                        var mmsg = JsonUtility.FromJson<MatchJoinedInfosMessage> (msg.data);
                        foreach (var item in mmsg.toSpawn) _gameMaster.OnReceiveData (item);
                        break;
                    }
                case "gamedata":
                    {
                        var dat = JsonUtility.FromJson<GlobalGameData> (msg.data);
                        _gameMaster.OnReceiveData (dat);
                        break;
                    }
            }

            // Trouble in here
            _ready = true;
        }

        public void Stop () {
            _gameMaster.Stop ();
        }

        public void Send (string type, object data) {
            Client.Send (new GlobalMessage (type, JsonUtility.ToJson (data)));
        }

        public void SendGameData (string type, int objectId, object data) {
            Client.Send (new GlobalMessage ("gamedata",
                JsonUtility.ToJson (new GlobalGameData (type, objectId, JsonUtility.ToJson (data)))));
        }

        #region Instance

        private static ClientHandler _instance;

        private ClientHandler () {
            _world = World.DefaultGameObjectInjectionWorld;
        }

        public static ClientHandler Get () {
            if (_instance == null) _instance = new ClientHandler ();
            return _instance;
        }

        #endregion

        #region Server Connection

        public MyUdpClient Client;
        private ClientGameMaster _gameMaster;

        public void Start (ServerDatas serverDatas, ServerDatas sdatas, Func<ClientGameMaster> createGameMaster, bool debug = false) {
            Client = new MyUdpClient ();
            this.debug = debug;
            Client.Start (serverDatas.address, serverDatas.port, GetReady, debug);
            _gameMaster = createGameMaster ();
            _gameMaster.Init (this);
        }

        #endregion
    }
}