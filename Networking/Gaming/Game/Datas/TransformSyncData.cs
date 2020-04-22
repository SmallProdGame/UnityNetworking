using System;
using UnityEngine;
using Unity.Entities;

namespace SmallProdGame.Networking.Gaming.Game
{
    [Serializable]
    public struct TransformSyncData : IComponentData
    {
        public enum PosMode
        {
            Translation
        }

        [Range(0.1f, 60f)] public float syncBySeconds;

        [Header("Position")] public bool syncPosition;
        public PosMode syncPosMode;
        public bool interpolatePosition;

        [HideInInspector] public float posX;
        [HideInInspector] public float posY;
        [HideInInspector] public float posZ;
        [HideInInspector] public float oposX;
        [HideInInspector] public float oposY;
        [HideInInspector] public float oposZ;

        public enum RotMode
        {
            Euler
        }

        [Header("Rotation")] public bool syncRotation;
        public RotMode syncRotMode;
        public bool interpolateRotation;

        [HideInInspector] public float rotX;
        [HideInInspector] public float rotY;
        [HideInInspector] public float rotZ;
        [HideInInspector] public float rotW;
        [HideInInspector] public float orotX;
        [HideInInspector] public float orotY;
        [HideInInspector] public float orotZ;

        [HideInInspector] public float timeTillLastSync;
        [HideInInspector] public float syncTime;
    }
}