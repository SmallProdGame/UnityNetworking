using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class Float4
    {
        public float w;
        public float x;
        public float y;
        public float z;

        public Float4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }

    [Serializable]
    public class SyncTransformMessage
    {
        public bool isPos;
        public bool isRot;
        public Float4 pos;
        public Float4 rot;

        public SyncTransformMessage(Float4 pos, Float4 rot, bool isPos, bool isRot)
        {
            this.pos = pos;
            this.rot = rot;
            this.isPos = isPos;
            this.isRot = isRot;
        }
    }
}