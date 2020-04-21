using SmallProdGame.Networking.Default;
using SmallProdGame.Networking.Gaming.Core.Client;
using SmallProdGame.Networking.Gaming.Messages;
using SmallProdGame.Networking.Gaming.Core.Server;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

namespace SmallProdGame.Networking.Gaming.Game
{
    [AlwaysSynchronizeSystem]
    public class TransformSyncSystem : JobComponentSystem
    {
        private ClientHandler _clientHandler;
        private bool _isServer;
        private GameServerHandler _serverHandler;

        protected override void OnCreate()
        {
            base.OnCreate();
            if (NetworkInfos.IsServer)
            {
                _isServer = true;
                _serverHandler = GameServerHandler.Get();
            }
            else
            {
                _isServer = false;
                _clientHandler = ClientHandler.Get();
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float deltaTime = Time.DeltaTime;
            var isServer = _isServer;
            var clientHandler = _clientHandler;
            var serverHandler = _serverHandler;
            Entities.WithoutBurst().ForEach(
                (ref TransformSyncData syncData, ref Translation translation, ref RotationEulerXYZ rot, in IdentityData identity, in Entity entity) =>
                {
                    syncData.timeTillLastSync += deltaTime;
                    if (identity.hasLocalAuthority)
                    {
                        var msg = new SyncTransformMessage(null, null, false, false);
                        if (syncData.timeTillLastSync >= syncData.syncTime)
                        {
                            syncData.timeTillLastSync = 0f;
                            if (syncData.syncPosition)
                            {
                                var oldPos = new Vector3(syncData.posX, syncData.posY, syncData.posZ);
                                if (Vector3.Distance(oldPos, translation.Value) > 0.1f)
                                {
                                    msg.pos = new Float4(translation.Value.x, translation.Value.y, translation.Value.z,
                                        0);
                                    syncData.posX = translation.Value.x;
                                    syncData.posY = translation.Value.y;
                                    syncData.posZ = translation.Value.z;
                                    msg.isPos = true;
                                }
                            }

                            if (syncData.syncRotation)
                            {
                                var oldRot = new Vector3(syncData.rotX, syncData.rotY, syncData.rotZ);
                                if (Vector3.Distance(oldRot, rot.Value) > 0.01f)
                                {
                                    msg.rot = new Float4(rot.Value.x, rot.Value.y, rot.Value.z, 0);
                                    syncData.rotX = rot.Value.x;
                                    syncData.rotY = rot.Value.y;
                                    syncData.rotZ = rot.Value.z;
                                    msg.isRot = true;
                                }
                            }
                        }

                        if (msg.isPos || msg.isRot)
                        {
                            if (isServer)
                                serverHandler.SendToMatch("synctransform", identity.objectId, msg, identity.matchId);
                            else
                                clientHandler.SendGameData("synctransform", identity.objectId, msg);
                        }
                    }
                    else
                    {
                        var t = syncData.timeTillLastSync / syncData.syncTime;
                        if (syncData.syncPosition)
                            if (syncData.syncPosMode == TransformSyncData.PosMode.Translation)
                            {
                                if (syncData.interpolatePosition && !isServer)
                                    translation.Value =
                                    Vector3.Slerp(new Vector3(syncData.oposX, syncData.oposY, syncData.oposZ),
                                        new Vector3(syncData.posX, syncData.posY, syncData.posZ), t);
                                else
                                    translation.Value = new float3(syncData.posX, syncData.posY, syncData.posZ);
                            }

                        if (syncData.syncRotation)
                            if (syncData.syncRotMode == TransformSyncData.RotMode.Euler)
                            {
                                if (syncData.interpolateRotation && !isServer)
                                    rot.Value = Vector3.Slerp(
                                        new Vector3(syncData.orotX, syncData.orotY, syncData.orotZ),
                                        new Vector3(syncData.rotX, syncData.rotY, syncData.rotZ), t);
                                else
                                    rot.Value = new float3(syncData.rotX, syncData.rotY, syncData.rotZ);
                            }
                    }
                }).Run();
            return default;
        }
    }
}