using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace SmallProdGame.Networking.Gaming.Game
{
    public class SpawnPositionComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] public SpawnPositionData positionData;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, positionData);
            dstManager.AddComponentData(entity, new RotationEulerXYZ());
        }
    }
}