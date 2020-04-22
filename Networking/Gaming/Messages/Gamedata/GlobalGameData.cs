using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class GlobalGameData
    {
        public string data;
        public int objectId;
        public string type;

        public GlobalGameData(string type, int objectId, string data)
        {
            this.type = type;
            this.objectId = objectId;
            this.data = data;
        }
    }
}