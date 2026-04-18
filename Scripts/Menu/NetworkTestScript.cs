using Unity.Netcode;
using UnityEngine;

public class NetworkTestScript : NetworkBehaviour
{
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendMessageServerRpc()
    {
        print("YOOOOOOOO!");
    }
}
