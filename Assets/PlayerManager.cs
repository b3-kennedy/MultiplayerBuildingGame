using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{

    public static PlayerManager Instance;

    public GameObject player;
    public GameObject cameraHolder;

    public GameObject client1Player;
    public GameObject client1Holder;

    public GameObject client2Player;
    public GameObject client2Holder;

    public NetworkVariable<ulong> player1NetId;
    public NetworkVariable<ulong> player2NetId;
    public NetworkVariable<ulong> player3NetId;
    public NetworkVariable<ulong> player4NetId;

    public NetworkVariable<ulong> player1HolderId;
    public NetworkVariable<ulong> player2HolderId;


    // Start is called before the first frame update
    void Start()
    {

        Instance = this;

        if(NetworkManager.Singleton.LocalClientId == 0)
        {
            SpawnPlayer1ServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else if(NetworkManager.Singleton.LocalClientId == 1)
        {
            SpawnPlayer2ServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayer1ServerRpc(ulong clientId)
    {
        client1Player = Instantiate(player);
        client1Player.name = "Player1";
        client1Player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        client1Holder = Instantiate(cameraHolder);
        client1Holder.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        player1NetId.Value = client1Player.GetComponent<NetworkObject>().NetworkObjectId;
        player1HolderId.Value = client1Holder.GetComponent<NetworkObject>().NetworkObjectId;

        SpawnPlayerClientRpc(clientId,client1Player.GetComponent<NetworkObject>().NetworkObjectId, client1Holder.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayer2ServerRpc(ulong clientId)
    {
        client2Player = Instantiate(player);
        client2Player.name = "Player2";
        client2Player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        client2Holder = Instantiate(cameraHolder);
        client2Holder.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        player2NetId.Value = client2Player.GetComponent<NetworkObject>().NetworkObjectId;
        player2HolderId.Value = client2Holder.GetComponent<NetworkObject>().NetworkObjectId;

        SpawnPlayerClientRpc(clientId,client2Player.GetComponent<NetworkObject>().NetworkObjectId, client2Holder.GetComponent<NetworkObject>().NetworkObjectId);

        
    }

    [ClientRpc]
    void SpawnPlayerClientRpc(ulong clientId, ulong playerObjId, ulong holderObjId)
    {

        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerObjId, out var player))
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(holderObjId, out var holder))
            {

                switch (clientId)
                {
                    case 0:
                        if (client1Player == null)
                        {
                            client1Player = player.gameObject;
                        }
                        break;
                    case 1:
                        if (client2Player == null)
                        {
                            client2Player = player.gameObject;
                        }
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                }

                player.name = "Player" + (clientId+1).ToString();
                player.GetComponent<PlayerInterfaceManager>().holder = holder.gameObject;
                player.GetComponent<PlayerLook>().cam = holder.transform.GetChild(1);
                holder.GetComponent<MoveCamera>().camPos = player.GetComponent<PlayerInterfaceManager>().camPos;
                player.GetComponent<PlayerLook>().player = player.gameObject;
                player.GetComponent<PlayerLook>().orientation = player.GetComponent<PlayerMovement>().orientation;
                player.GetComponent<PlayerLook>().Assign();
            }

        }


        if(NetworkManager.Singleton.LocalClientId == 0)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2NetId.Value, out var player2))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2HolderId.Value, out var player2Holder))
                {
                    player2.name = "Player2";
                    player2.GetComponent<PlayerLook>().enabled = false;
                    player2Holder.GetComponent<MoveCamera>().camPos = player2.GetComponent<PlayerInterfaceManager>().camPos;
                    player2Holder.transform.GetChild(1).GetComponent<Camera>().enabled = false;
                    player2Holder.transform.GetChild(1).GetComponent<AudioListener>().enabled = false;
                    player2.GetComponent<InventoryManager>().enabled = false;
                }

            }
        }
        else if(NetworkManager.Singleton.LocalClientId == 1)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1NetId.Value, out var player1))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1HolderId.Value, out var player1Holder))
                {
                    player1.name = "Player1";
                    player1.GetComponent<PlayerLook>().enabled = false;
                    player1Holder.GetComponent<MoveCamera>().camPos = player1.GetComponent<PlayerInterfaceManager>().camPos;
                    player1Holder.transform.GetChild(1).GetComponent<Camera>().enabled = false;
                    player1Holder.transform.GetChild(1).GetComponent<AudioListener>().enabled = false;
                    player1.GetComponent<InventoryManager>().enabled = false;
                }

            }
        }

    }

    public GameObject GetClientPlayer(ulong id)
    {
        switch (id)
        {
            case 0:
                return client1Player;
            case 1:
                return client2Player;
            case 2:
                break;
            case 3:
                break;
            default:
                break;
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
