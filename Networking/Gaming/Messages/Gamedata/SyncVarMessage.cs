using System;

namespace SmallProdGame.Networking.Gaming.Messages
{
    [Serializable]
    public class SyncVarMessage
    {
        public string field;
        public string type;
        public object value;

        public SyncVarMessage(string type, string field, object value)
        {
            this.type = type;
            this.field = field;
            this.value = value;
        }
    }
}