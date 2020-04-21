using System;
using System.Collections;
using System.Collections.Generic;
using SmallProdGame.Networking.Default;
using SmallProdGame.Networking.Gaming.Game;
using SmallProdGame.Networking.Gaming.Messages;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevel;
using Random = System.Random;
using SmallProdGame.Utils;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace SmallProdGame.Networking.Gaming.Core.Server {
    public class Match : Debugger {
        private int _curSpawnPoint;
        private readonly Random _rand = new Random ();
        public BlobAssetStore blobAssetStore;
        public ServerGameMaster gameMaster;
        public GameServerHandler handler;
        public string map;
        public int matchId;
        public int nbPlayers;
        public List<Player> players;
        public Dictionary<int, EntityEntry> spawnedObjects = new Dictionary<int, EntityEntry> ();
        public List<Entity> spawnPoints;
        public string type;
        public World world;

        public Match (Func<Match, ServerGameMaster> createGameMaster, string createMatchMessage) {
            gameMaster = createGameMaster (this);
            gameMaster.CreateMatch (createMatchMessage);
            blobAssetStore = new BlobAssetStore ();

            nbPlayers = gameMaster.nbPlayers;
            players = new List<Player> ();
            handler = GameServerHandler.Get ();
            InitWorld (matchId.ToString ());
            gameMaster.OnMatchReady ();
        }

        public void StartCoroutine (IEnumerator enumerator) {
            ManagerComponent.Get ().StartCoroutine (enumerator);
        }

        private void InitWorld (string name) {
            DefaultWorldInitialization.Initialize (name, false);
            world = World.DefaultGameObjectInjectionWorld;
            world.GetExistingSystem (typeof (RenderMeshSystemV2)).Enabled = false;
            // Let's instantiate the Map
            var themap = ManagerComponent.Get ().maps.Find (m => m.name == map);
            EntityManager manager = world.EntityManager;
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld (world, blobAssetStore);
            Entity entity = GameObjectConversionUtility.ConvertGameObjectHierarchy (themap.mapObj, settings);
            Entity spawn = manager.Instantiate (entity);
            GetSpawnPoints ();
        }

        public void OnPlayerJoin (Player player) {
            EntityManager entityManager = world.EntityManager;
            Translation pos = entityManager.GetComponentData<Translation> (spawnPoints[_curSpawnPoint]);
            Rotation rot = entityManager.GetComponentData<Rotation> (spawnPoints[_curSpawnPoint]);
            gameMaster.OnPlayerJoin (player, pos.Value, rot.Value);
            _curSpawnPoint++;
            if (_curSpawnPoint >= spawnPoints.Count) _curSpawnPoint = 0;
        }

        public void GetSpawnPoints () {
            spawnPoints = new List<Entity> ();
            EntityQuery entityQuery = world.EntityManager.CreateEntityQuery (typeof (SpawnPositionData));
            NativeArray<Entity> sp = entityQuery.ToEntityArray (Allocator.TempJob);
            foreach (Entity item in sp) spawnPoints.Add (item);
            sp.Dispose ();
        }

        public IEnumerator Delete () {
            var gameServerHandler = GameServerHandler.Get ();
            foreach (var p in players) gameServerHandler.clients.Remove (p.client.ep.ToString ());
            gameMaster.OnEveryPlayerLeaved ();
            blobAssetStore.Dispose ();
            world.QuitUpdate = true;
            ScriptBehaviourUpdateOrder.SetPlayerLoop (PlayerLoop.GetDefaultPlayerLoop ());
            world.EntityManager.CompleteAllJobs ();
            world.EntityManager.DestroyEntity (world.EntityManager.GetAllEntities ());
            yield return new WaitForEndOfFrame ();
            world.Dispose ();
            gameServerHandler.matches.Remove (matchId);
        }

        public void EndMatch () {
            Broadcast ("match_end", new NullMessage ());
            ManagerComponent.Get ().StartCoroutine (Delete ());
            var gameServerHandler = GameServerHandler.Get ();
            gameServerHandler.SendMm ("match_end", new MatchEndMessage (matchId));
        }

        public void SendMessageToPlayer (int objectId, string type, object datas, Player player) {
            handler.SendGameData (type, objectId, datas, player.client.ep);
        }

        public void BroadcastGameData (int objectId, string type, object datas) {
            foreach (var item in players) handler.SendGameData (type, objectId, datas, item.client.ep);
        }

        public void Broadcast (string type, object datas) {
            foreach (var item in players) handler.Send (type, datas, item.client.ep);
        }

        public void BroadcastExceptPlayer (string type, object datas, Player player) {
            foreach (var item in players)
                if (item != player)
                    handler.Send (type, datas, item.client.ep);
        }

        public void BroadcastGameDataExceptPlayer (int objectId, string type, object datas, Player player) {
            foreach (var item in players)
                if (item != player)
                    handler.SendGameData (type, objectId, datas, item.client.ep);
        }

        private int GetNewEntityId () {
            return _rand.Next (1, 100000000);
        }

        public EntityEntry SpawnWithPlayerAuthority (Player player, GameObject obj, Vector3 pos, Quaternion rot) {
            return SpawnFromGameObject (obj, pos, rot, player);
        }

        public EntityEntry SpawnWithServerAuthority (GameObject obj, Vector3 pos, Quaternion rot) {
            return SpawnFromGameObject (obj, pos, rot);
        }

        public void Destroy (int objectId) {
            if (!spawnedObjects.ContainsKey (objectId)) return;
            world.EntityManager.DestroyEntity (spawnedObjects[objectId].entity);
            spawnedObjects.Remove (objectId);
            foreach (var item in players)
                handler.SendGameData ("destroy", objectId, new DestroyMessage (), item.client.ep);
        }

        private EntityEntry SpawnFromGameObject (GameObject obj, Vector3 pos, Quaternion rot, Player player = null) {
            var manager = ManagerComponent.Get ();
            var index = manager.spawnableObjects.IndexOf (obj);
            if (index == -1) return null;
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld (world, blobAssetStore);
            Entity e = GameObjectConversionUtility.ConvertGameObjectHierarchy (manager.spawnableObjects[index],
                settings);
            return Spawn (e, player == null, index, pos, rot, player != null ? player.key : null);
        }

        private EntityEntry Spawn (Entity e, bool serverAuth, int objIndex, Vector3 pos, Quaternion rot,
            string playerKey = null) {
            var id = GetNewEntityId ();
            var msg = new SpawnMessage (objIndex, false, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
            foreach (var item in players) {
                if (!serverAuth && item.key == playerKey) msg.hasAuthority = true;
                handler.SendGameData ("spawn", id, msg, item.client.ep);
                msg.hasAuthority = false;
            }

            Entity entity = world.EntityManager.Instantiate (e);
            world.EntityManager.AddComponentData (entity,
                new IdentityData { hasLocalAuthority = serverAuth, matchId = matchId, objectId = id });
            world.EntityManager.AddComponentData (entity, new Translation { Value = pos });
            world.EntityManager.AddComponentData (entity, new Rotation { Value = rot });

            var entityEntry = new EntityEntry {
                objectId = id,
                entity = entity,
                serverAuthority = serverAuth,
                ready = true,
                playerKey = playerKey,
                objectIndex = objIndex,
                pos = pos,
                rot = rot
            };
            spawnedObjects.Add (id, entityEntry);
            return entityEntry;
        }

        public object SendNewPlayerInformations () {
            var msg = new List<GlobalGameData> ();
            foreach (var item in spawnedObjects) {
                Vector3 pos = item.Value.pos;
                Quaternion rot = item.Value.rot;
                msg.Add (new GlobalGameData ("spawn", item.Key,
                    JsonUtility.ToJson (new SpawnMessage (item.Value.objectIndex, false, pos.x, pos.y, pos.z, rot.x,
                        rot.y, rot.z, rot.w))));
            }

            return new MatchJoinedInfosMessage (msg.ToArray ());
        }

        public class EntityEntry {
            public Entity entity;
            public int objectId;
            public int objectIndex;
            public string playerKey;
            public Vector3 pos;
            public bool ready;
            public Quaternion rot;
            public bool serverAuthority;
        }
    }
}