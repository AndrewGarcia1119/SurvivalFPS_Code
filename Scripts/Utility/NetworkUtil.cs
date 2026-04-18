using Unity.Netcode;
using UnityEngine;

namespace ShooterSurvival.Util
{
    public class NetworkUtil
    {
        public static NetworkObject GetLocalPlayer()
        {
            if (!NetworkManager.Singleton) return null;
            return NetworkManager.Singleton.LocalClient.PlayerObject;
        }

        public static NetworkObject GetPlayerById(ulong clientId)
        {
            if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
                return NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            else
                return null;
        }
    }

}