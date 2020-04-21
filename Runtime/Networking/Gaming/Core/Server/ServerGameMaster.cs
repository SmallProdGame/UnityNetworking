using System.Collections.Generic;
using SmallProdGame.Networking.Default;
using SmallProdGame.Networking.Gaming.Game;
using SmallProdGame.Networking.Gaming.Messages;
using SmallProdGame.Utils;
using Unity.Entities;
using UnityEngine;

namespace SmallProdGame.Networking.Gaming.Core.Server {
    public class ServerGameMaster : SmallProdGame.Utils.EventHandler {
        private readonly Queue<MainThreadQueueElement> _mainThreadQueue;
        protected List<string> keys;
        protected ManagerComponent managerComponent;

        protected Match match;
        public int nbPlayers;

        public ServerGameMaster (Match match) {
            this.match = match;
            managerComponent = ManagerComponent.Get ();
            _mainThreadQueue = new Queue<MainThreadQueueElement> ();
        }

        public void EmitMainThread (string type, object obj) {
            _mainThreadQueue.Enqueue (new MainThreadQueueElement { type = type, parameters = obj });
        }

        public void ExecuteQueue () {
            while (_mainThreadQueue.Count > 0) {
                var elem = _mainThreadQueue.Dequeue ();
                Emit (elem.type, elem.parameters);
            }
        }

        protected virtual void SyncTransform (Entity e, Vector3 pos, Vector3 rot) {
            EntityManager entityManager = match.world.EntityManager;
            TransformSyncData sync = entityManager.GetComponentData<TransformSyncData> (e);
            sync.posX = pos.x;
            sync.posY = pos.y;
            sync.posZ = pos.z;
            sync.oposX = pos.x;
            sync.oposY = pos.y;
            sync.oposZ = pos.z;
            sync.rotX = rot.x;
            sync.rotY = rot.y;
            sync.rotZ = rot.z;
            sync.orotX = rot.x;
            sync.orotY = rot.y;
            sync.orotZ = rot.z;
            entityManager.SetComponentData (e, sync);
        }

        public virtual void CreateMatch (string data) {
            var msg = JsonUtility.FromJson<CreateMatchMessage> (data);
            match.matchId = msg.matchId;
            match.map = msg.map;
            match.type = msg.type;
            keys = new List<string> (msg.keys);
            nbPlayers = keys.Count;
        }

        public virtual bool AddPlayer (Player player, string key) {
            if (keys.Contains (key)) {
                keys.Remove (key);
                return true;
            }

            return false;
        }

        public virtual void OnMatchReady () { }

        public virtual void OnPlayerJoin (Player player, Vector3 pos, Quaternion rot) {
            match.SpawnWithPlayerAuthority (player, managerComponent.spawnableObjects[0], pos, rot);
            if (keys.Count == 0) OnEveryPlayerJoined ();
        }

        public virtual void OnEveryPlayerJoined () { }

        public virtual void OnPlayerReady (Player player) {
            player.isReady = true;
            if (keys.Count == 0 && match.players.Find (p => !p.isReady) == null) OnEveryPlayerReady ();
        }

        public virtual void OnEveryPlayerReady () {
            match.Broadcast ("match_start", new NullMessage ());
        }

        public virtual void OnStartGame () { }

        public virtual void OnPlayerLeave (Player player) { }

        public virtual void OnEveryPlayerLeaved () { }

        public virtual void OnNewMessage (GlobalGameData msg, Player player) {
            switch (msg.type) {
                case "synctransform":
                    {
                        SyncTransformMessage syncTransformMessage = JsonUtility.FromJson<SyncTransformMessage> (msg.data);
                        var objExist = match.spawnedObjects.ContainsKey (msg.objectId);
                        if (objExist) {
                            if (!match.spawnedObjects[msg.objectId].ready) return;
                            Entity e = match.spawnedObjects[msg.objectId].entity;
                            managerComponent.SyncTransform (match.world, e, syncTransformMessage);
                            match.BroadcastExceptPlayer ("gamedata", msg, player);
                        }

                        break;
                    }
            }
        }

        public virtual void EndMatch () {
            match.EndMatch ();
        }

        protected override void InitEvents () { }

        private struct MainThreadQueueElement {
            public string type;
            public object parameters;
        }
    }
}