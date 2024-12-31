using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerCard : NetworkBehaviour
{
    public string playerName;


    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SetLocalName();
    }

    public void SetLocalName()
    {
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerNames.Instance.localName;
        SetPlayerNameServerRpc(PlayerNames.Instance.localName, GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.Singleton.LocalClientId);
    }

    void ClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                NetworkObject playerObject = client.Value.PlayerObject;
                SetPlayerNameServerRpc(PlayerNames.Instance.playerNames[client.Key], playerObject.NetworkObjectId, playerObject.OwnerClientId);
            }
        }

    }

    private void Update()
    {
        if (!IsOwner) return;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name, ulong networkObjId, ulong clientId)
    {
        SetPlayerNameClientRpc(name, networkObjId, clientId);
        PlayerNames.Instance.playerNames[clientId] = name;
    }

    [ClientRpc]
    void SetPlayerNameClientRpc(string name, ulong networkObjId, ulong clientId)
    {
        PlayerNames.Instance.playerNames[clientId] = name;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjId, out var card))
        {
            card.GetComponent<PlayerCard>().playerName = name;
            card.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        }
    }
}
