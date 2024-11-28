using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;
    public Transform[] positions;
    public GameObject playerCard;

    public List<GameObject> spawnedCards;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        if (IsServer)
        {
            GetPlayerCardServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        
    }


    // This will store the callback reference for the client connection event.
    private void OnEnable()
    {
        // Subscribe to the client connection event
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    [ServerRpc(RequireOwnership = false)]
    void GetPlayerCardServerRpc(ulong clientId)
    {
        if(NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            client.PlayerObject.transform.SetParent(positions[clientId]);
            client.PlayerObject.transform.position = Vector3.zero;
            client.PlayerObject.transform.localScale = Vector3.one;

            ParentClientCardClientRpc(client.PlayerObject.GetComponent<NetworkObject>().NetworkObjectId, clientId);

        }

        


    }

    [ClientRpc]
    void ParentClientCardClientRpc(ulong playerObjId, ulong clientId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerObjId, out var playerObj))
        {
            playerObj.transform.localPosition = Vector3.zero;            
        }

        foreach (var obj in positions)
        {
            if(obj.transform.childCount > 0)
            {
                obj.GetChild(0).transform.localScale = Vector3.one;
            }
            
        }
    }


    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }




    // This method is called when a client connects to the server
    private void OnClientConnected(ulong clientId)
    {
        GetPlayerCardServerRpc(clientId);

     
    }



}
