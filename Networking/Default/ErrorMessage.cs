using System;

namespace SmallProdGame.Networking.Default
{
    [Serializable]
    public class ErrorMessage
    {
        public ErrorMessage(int error, string infos)
        {
            this.error = error;
            this.infos = infos;
        }

        public int error;
        public string infos;
    }
}