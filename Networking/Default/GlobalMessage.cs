using System;

namespace SmallProdGame.Networking.Default
{
    [Serializable]
    public class GlobalMessage
    {
        public string data;
        public string type;

        public GlobalMessage(string type, string data)
        {
            this.type = type;
            this.data = data;
        }
    }
}