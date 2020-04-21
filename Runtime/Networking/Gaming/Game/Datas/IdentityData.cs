using System;
using UnityEngine;
using Unity.Entities;

namespace SmallProdGame.Networking.Gaming.Game
{
    [Serializable]
    public struct IdentityData : IComponentData
    {
        [HideInInspector] public bool hasLocalAuthority;
        [HideInInspector] public int matchId;
        [HideInInspector] public int objectId;
    }
}