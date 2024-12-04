using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class Chest : NetworkBehaviour
{

    public NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);
    public int slotCount = 10;
    [HideInInspector] public GameObject interactingPlayer;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenBackpackTab()
    {

    }

    void OpenChestTab()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenChestServerRpc(ulong clientId)
    {
        isOpen.Value = true;
        OpenedChestClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void CloseChestServerRpc()
    {
        isOpen.Value = false;
    }


    [ClientRpc]
    void OpenedChestClientRpc(ClientRpcParams clientRpcParams = default)
    {
        var player = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
    }
}
