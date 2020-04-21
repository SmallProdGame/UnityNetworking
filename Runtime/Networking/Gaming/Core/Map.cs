using UnityEngine;

namespace SmallProdGame.Networking.Gaming.Core
{
    [CreateAssetMenu(fileName = "New map", menuName = "Map")]
    public class Map : ScriptableObject
    {
        public GameObject mapObj;
        public new string name;
        public string sceneName;
    }
}