using UnityEngine;
using Unity.Entities;

namespace SmallProdGame.Networking.Gaming.Game
{
    public class IdentityDataComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] public IdentityData identityData;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, identityData);
        }
    }
}