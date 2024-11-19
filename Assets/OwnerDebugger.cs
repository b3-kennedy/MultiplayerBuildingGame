using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OwnerDebugger : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log(gameObject.GetComponent<NetworkObject>().OwnerClientId);
    }
}
