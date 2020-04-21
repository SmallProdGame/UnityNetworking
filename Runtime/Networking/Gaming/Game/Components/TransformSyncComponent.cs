using UnityEngine;
using Unity.Entities;

namespace SmallProdGame.Networking.Gaming.Game
{
    public class TransformSyncComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] public TransformSyncData syncData;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            syncData.timeTillLastSync = 0;
            syncData.syncTime = 1f / syncData.syncBySeconds;
            dstManager.AddComponentData(entity, syncData);
        }
    }
}