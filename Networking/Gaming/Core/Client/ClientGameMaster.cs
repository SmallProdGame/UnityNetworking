using System.Collections.Generic;
using SmallProdGame.Networking.Gaming.Game;
using SmallProdGame.Networking.Gaming.Messages;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace SmallProdGame.Networking.Gaming.Core.Client {
    public class ClientGameMaster : SmallProdGame.Utils.EventHandler {
        protected ClientHandler clientHandler;

        protected ManagerComponent managerComponent;
        protected int playerEntityId = -1;
        protected Dictionary<int, Entity> spawnedEntities = new Dictionary<int, Entity> ();
        protected BlobAssetStore store = new BlobAssetStore ();
        protected World world;

        public void Init (ClientHandler clientHandler) {
            this.clientHandler = clientHandler;
        }

        public virtual void Reinit () {
            playerEntityId = -1;
            EntityManager entityManager = world.EntityManager;
            foreach (KeyValuePair<int, Entity> item in spawnedEntities) entityManager.DestroyEntity (item.Value);
            spawnedEntities.Clear ();
        }

        protected override void InitEvents () {
            // Nothing yet
        }

        public virtual void LeaveMatch () {
            ClientHandler.Get ().Send ("leavematch", null);
        }

        protected virtual void OnSpawnLocalPlayer (Entity playerEntity, int id, Vector3 pos, Quaternion rot) { }

        protected virtual void OnSpawnOtherPlayer (Entity playerEntity, int id, Vector3 pos, Quaternion rot) { }

        protected virtual void OnDestroy (int id) { }

        protected virtual void SyncTransform (Entity e, Vector3 pos, Vector3 rot) {
            EntityManager entityManager = world.EntityManager;
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

        public virtual void Stop () {
            store.Dispose ();
        }

        public virtual void OnReceiveData (GlobalGameData msg) {
            switch (msg.type) {
                case "spawn":
                    {
                        var spawnMessage = JsonUtility.FromJson<SpawnMessage> (msg.data);
                        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld (world, store);
                        Entity e = GameObjectConversionUtility.ConvertGameObjectHierarchy (
                            managerComponent.spawnableObjects[spawnMessage.index], settings);
                        EntityManager manager = world.EntityManager;
                        Entity entity = manager.Instantiate (e);
                        var pos = new Vector3 (spawnMessage.posX, spawnMessage.posY, spawnMessage.posZ);
                        var rot = new Quaternion (spawnMessage.rotX, spawnMessage.rotY, spawnMessage.rotZ,
                            spawnMessage.rotW);
                        manager.AddComponentData (entity,
                            new IdentityData { hasLocalAuthority = spawnMessage.hasAuthority, matchId = 0, objectId = msg.objectId });
                        manager.AddComponentData (entity, new Translation { Value = pos });
                        manager.AddComponentData (entity, new Rotation { Value = rot });
                        if (playerEntityId == -1 && spawnMessage.hasAuthority) {
                            playerEntityId = msg.objectId;
                            OnSpawnLocalPlayer (entity, msg.objectId, pos, rot);
                        } else if (spawnMessage.index == 0) {
                            OnSpawnOtherPlayer (entity, msg.objectId, pos, rot);
                        }

                        spawnedEntities.Add (msg.objectId, entity);
                        break;
                    }
                case "destroy":
                    {
                        var destroyMessage = JsonUtility.FromJson<DestroyMessage> (msg.data);
                        var objExist = spawnedEntities.ContainsKey (msg.objectId);
                        if (objExist) {
                            Entity e = spawnedEntities[msg.objectId];
                            managerComponent.DestroyObject (world, e);
                            OnDestroy (msg.objectId);
                            spawnedEntities.Remove (msg.objectId);
                        }

                        break;
                    }
                case "synctransform":
                    {
                        var syncTransformMessage = JsonUtility.FromJson<SyncTransformMessage> (msg.data);
                        var objExist = spawnedEntities.ContainsKey (msg.objectId);
                        if (objExist) {
                            Entity e = spawnedEntities[msg.objectId];
                            managerComponent.SyncTransform (world, e, syncTransformMessage);
                        }

                        break;
                    }
            }
        }

        #region Instance

        private static ClientGameMaster _instance;

        protected ClientGameMaster () {
            managerComponent = ManagerComponent.Get ();
            world = World.DefaultGameObjectInjectionWorld;
        }

        public static ClientGameMaster Get () {
            if (_instance == null) _instance = new ClientGameMaster ();
            return _instance;
        }

        #endregion
    }
}