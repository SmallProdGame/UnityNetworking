using System.Collections;
using System.Collections.Generic;
using SmallProdGame.Networking.Default;
using SmallProdGame.Networking.Gaming.Core;
using SmallProdGame.Networking.Gaming.Core.Client;
using SmallProdGame.Networking.Gaming.Core.Server;
using SmallProdGame.Networking.Gaming.Messages;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace SmallProdGame.Networking.Gaming.Game {
    public class ManagerComponent : MonoBehaviour {
        private World _world;
        protected ClientHandler clientHandler;

        public bool forceServer;
        public bool debug;
        [SerializeField] public List<Map> maps = new List<Map> ();

        [Header ("Matchmaking")]
        [SerializeField]
        public ServerDatas mmServerDatas;

        [Header ("Gaming")][SerializeField] public ServerDatas serverDatas;

        protected GameServerHandler serverHandler;
        public List<GameObject> spawnableObjects = new List<GameObject> ();

        public int GetEntityId (GameObject obj) {
            return spawnableObjects.IndexOf (obj);
        }

        public Match GetMatchById (int id) {
            if (!NetworkInfos.IsServer) return null;
            if (!serverHandler.matches.ContainsKey (id)) return null;
            return serverHandler.matches[id];
        }

        protected virtual void Start () {
            InitInstance ();
#if UNITY_SERVER
            StartAsGameServer ();
#elif UNITY_EDITOR
            if (forceServer)
                StartAsGameServer ();
            else
                StartAsClient ();
#else
            StartAsClient ();
#endif
        }

        public virtual void JoinMatch (int matchId, string key) {
            if (NetworkInfos.IsServer) return;
            clientHandler.Send ("join_match", new JoinMatchMessage (matchId, key));
        }

        public virtual void Ready () {
            if (NetworkInfos.IsServer) return;
            clientHandler.Send ("ready_match", new NullMessage ());
        }

        public void SendMessageToServer (string type, IdentityData identity) {
            if (NetworkInfos.IsServer) return;
            clientHandler.SendGameData (type, identity.objectId, null);
        }

        public void DestroyObject (World world, Entity entity) {
            world.EntityManager.DestroyEntity (entity);
        }

        public void SyncTransform (World world, Entity entity, SyncTransformMessage msg) {
            EntityManager manager = world.EntityManager;
            TransformSyncData transform = manager.GetComponentData<TransformSyncData> (entity);
            Translation translation = manager.GetComponentData<Translation> (entity);
            RotationEulerXYZ rot = manager.GetComponentData<RotationEulerXYZ> (entity);
            transform.timeTillLastSync = 0f;
            transform.oposX = translation.Value.x;
            transform.oposY = translation.Value.y;
            transform.oposZ = translation.Value.z;
            transform.orotX = rot.Value.x;
            transform.orotY = rot.Value.y;
            transform.orotZ = rot.Value.z;
            if (msg.isPos) {
                transform.posX = msg.pos.x;
                transform.posY = msg.pos.y;
                transform.posZ = msg.pos.z;
            }

            if (msg.isRot) {
                transform.rotX = msg.rot.x;
                transform.rotY = msg.rot.y;
                transform.rotZ = msg.rot.z;
                transform.rotW = msg.rot.w;
            }

            manager.SetComponentData (entity, transform);
        }

        public object EmitToGameMaster (int matchId, string type, object data) {
            if (!NetworkInfos.IsServer) return null;
            var match = GetMatchById (matchId);
            if (match == null) return null;
            return match.gameMaster.Emit (type, data);
        }

        public void EmitToGameMasterMainThread (int matchId, string type, object data) {
            if (!NetworkInfos.IsServer) return;
            var match = GetMatchById (matchId);
            if (match == null) return;
            match.gameMaster.EmitMainThread (type, data);
        }

        protected virtual ServerGameMaster CreateServerGameMaster (Match match) {
            return new ServerGameMaster (match);
        }

        protected virtual ClientGameMaster CreateClientGameMaster () {
            return ClientGameMaster.Get ();
        }

        private void StartAsGameServer () {
            NetworkInfos.IsServer = true;
            JobHandle.ScheduleBatchedJobs ();
            /*foreach (GameObject item in FindObjectsOfType<GameObject>())
            {
                if (item != gameObject)
                {
                    Destroy(item);
                }
            }*/
            serverHandler = GameServerHandler.Get ();
            serverHandler.Start (serverDatas, mmServerDatas, CreateServerGameMaster, debug);
            StartCoroutine (serverHandler.Client.GetDatas (serverHandler.OnReceiveMMData));
            StartCoroutine (serverHandler.Server.GetDatas (serverHandler.OnReceiveData));
            StartCoroutine (serverHandler.CoroutineHandler ());
        }

        public GameObject Spawn (GameObject obj, Vector3 pos, Quaternion rot) {
            return Instantiate (obj, pos, rot);
        }

        public void Remove (GameObject obj) {
            Destroy (obj);
        }

        public void Remove (Component obj) {
            Destroy (obj);
        }

        private void StartAsClient () {
            NetworkInfos.IsServer = false;
            clientHandler = ClientHandler.Get ();
            clientHandler.Start (serverDatas, mmServerDatas, CreateClientGameMaster, debug);
            _world = World.DefaultGameObjectInjectionWorld;
            StartCoroutine (clientHandler.Client.GetDatas (clientHandler.OnReceiveData));
        }

        private void OnApplicationQuit () {
            if (NetworkInfos.IsServer)
                serverHandler.Stop ();
            else
                clientHandler.Stop ();
            StartCoroutine (WaitQuit ());
        }

        private IEnumerator WaitQuit () {
            yield return new WaitForSeconds (3f);
        }

        #region Instance

        private static ManagerComponent _instance;

        private void InitInstance () {
            if (_instance == null)
                _instance = this;
            else
                Destroy (this);
        }

        public static ManagerComponent Get () {
            return _instance;
        }

        #endregion
    }
}