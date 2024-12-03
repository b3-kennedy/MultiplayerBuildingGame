using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RockObject : NetworkBehaviour
{
    public NetworkVariable<float> treeHealth = new NetworkVariable<float>();
    float localHealth;
    public Item stoneItem;
    public Item rockItem;

    private void Start()
    {
        localHealth = treeHealth.Value;
    }

    public void TakeDamageLocally(float damage)
    {
        localHealth -= damage;
        if (localHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ulong clientId)
    {
        treeHealth.Value -= damage;

        GiveWoodToClientRpc(5, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });

        GiveSticksToClientRpc(1, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });

        if (treeHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    [ClientRpc]
    void GiveWoodToClientRpc(float stone, ClientRpcParams clientRpcParams = default)
    {
        GameObject playerObject = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
        for (int i = 0; i < stone; i++)
        {
            playerObject.GetComponent<InventoryManager>().AddItem(stoneItem);
        }
    }

    [ClientRpc]
    void GiveSticksToClientRpc(float rocks, ClientRpcParams clientRpcParams = default)
    {
        GameObject playerObject = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
        for (int i = 0; i < rocks; i++)
        {
            playerObject.GetComponent<InventoryManager>().AddItem(rockItem);
        }
    }
}
