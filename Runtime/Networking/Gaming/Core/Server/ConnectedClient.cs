using System.Net;

namespace SmallProdGame.Networking.Gaming.Core.Server
{
    public class ConnectedClient
    {
        public IPEndPoint ep;
        public string id;
        public Match match;

        public ConnectedClient(IPEndPoint ep, Match match)
        {
            this.id = ep.ToString();
            this.ep = ep;
            this.match = match;
        }
    }
}