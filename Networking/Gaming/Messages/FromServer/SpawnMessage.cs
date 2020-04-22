using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class SpawnMessage
    {
        public bool hasAuthority;
        public int index;
        public float posX;
        public float posY;
        public float posZ;
        public float rotW;
        public float rotX;
        public float rotY;
        public float rotZ;

        public SpawnMessage(int index, bool hasAuthority, float posX, float posY, float posZ, float rotX, float rotY,
            float rotZ, float rotW)
        {
            this.index = index;
            this.hasAuthority = hasAuthority;
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            this.rotW = rotW;
        }
    }
}