using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerManager : NetworkBehaviour
{

    public static PlayerManager Instance;

    public GameObject player;
    public GameObject cameraHolder;

    public GameObject client1Player;
    public GameObject client1Holder;
    public GameObject client1NameCard;

    public GameObject client2Player;
    public GameObject client2Holder;
    public GameObject client2NameCard;

    public GameObject client3Player;
    public GameObject client3Holder;
    public GameObject client3NameCard;

    public GameObject client4Player;
    public GameObject client4Holder;
    public GameObject client4NameCard;

    public List<GameObject> clientHolders = new List<GameObject>();

    public NetworkVariable<ulong> player1NetId;
    public NetworkVariable<ulong> player2NetId;
    public NetworkVariable<ulong> player3NetId;
    public NetworkVariable<ulong> player4NetId;

    public NetworkVariable<ulong> player1HolderId;
    public NetworkVariable<ulong> player2HolderId;
    public NetworkVariable<ulong> player3HolderId;
    public NetworkVariable<ulong> player4HolderId;

    public List<GameObject> localClientToolbeltItems = new List<GameObject>();

    int spawnedPlayers = 0;


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
        else if(NetworkManager.Singleton.LocalClientId == 2)
        {
            SpawnPlayer3ServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else if(NetworkManager.Singleton.LocalClientId == 3)
        {
            SpawnPlayer4ServerRpc(NetworkManager.Singleton.LocalClientId);
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

        var nameCardText = client1Player.GetComponent<PlayerInterfaceManager>().nameCard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        nameCardText.text = PlayerNames.Instance.playerNames[0];

        player1NetId.Value = client1Player.GetComponent<NetworkObject>().NetworkObjectId;
        player1HolderId.Value = client1Holder.GetComponent<NetworkObject>().NetworkObjectId;

        EnableCameraComponentsClientRpc(client1Holder.GetComponent<NetworkObject>().NetworkObjectId, client1Player.GetComponent<NetworkObject>().NetworkObjectId ,new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong> { clientId }
            }
        });
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

        var nameCardText = client2Player.GetComponent<PlayerInterfaceManager>().nameCard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        nameCardText.text = PlayerNames.Instance.playerNames[1];

        player2NetId.Value = client2Player.GetComponent<NetworkObject>().NetworkObjectId;
        player2HolderId.Value = client2Holder.GetComponent<NetworkObject>().NetworkObjectId;

        EnableCameraComponentsClientRpc(client2Holder.GetComponent<NetworkObject>().NetworkObjectId, client2Player.GetComponent<NetworkObject>().NetworkObjectId
            ,new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong> { clientId }
            }
        });
        SpawnPlayerClientRpc(clientId,client2Player.GetComponent<NetworkObject>().NetworkObjectId, client2Holder.GetComponent<NetworkObject>().NetworkObjectId);



    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayer3ServerRpc(ulong clientId)
    {
        client3Player = Instantiate(player);
        client3Player.name = "Player3";
        client3Player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        client3Holder = Instantiate(cameraHolder);
        client3Holder.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        var nameCardText = client3Player.GetComponent<PlayerInterfaceManager>().nameCard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        nameCardText.text = PlayerNames.Instance.playerNames[2];

        player3NetId.Value = client3Player.GetComponent<NetworkObject>().NetworkObjectId;
        player3HolderId.Value = client3Holder.GetComponent<NetworkObject>().NetworkObjectId;

        EnableCameraComponentsClientRpc(client3Holder.GetComponent<NetworkObject>().NetworkObjectId, client3Player.GetComponent<NetworkObject>().NetworkObjectId
            ,new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong> { clientId }
            }
        });
        SpawnPlayerClientRpc(clientId, client3Player.GetComponent<NetworkObject>().NetworkObjectId, client3Holder.GetComponent<NetworkObject>().NetworkObjectId);



    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayer4ServerRpc(ulong clientId)
    {
        client4Player = Instantiate(player);
        client4Player.name = "Player4";
        client4Player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        client4Holder = Instantiate(cameraHolder);
        client4Holder.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        var nameCardText = client4Player.GetComponent<PlayerInterfaceManager>().nameCard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        nameCardText.text = PlayerNames.Instance.playerNames[3];

        player4NetId.Value = client4Player.GetComponent<NetworkObject>().NetworkObjectId;
        player4HolderId.Value = client4Holder.GetComponent<NetworkObject>().NetworkObjectId;



        EnableCameraComponentsClientRpc(client4Holder.GetComponent<NetworkObject>().NetworkObjectId, 
            client4Player.GetComponent<NetworkObject>().NetworkObjectId ,new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong> { clientId }
            }
        });

        SpawnPlayerClientRpc(clientId, client4Player.GetComponent<NetworkObject>().NetworkObjectId, client4Holder.GetComponent<NetworkObject>().NetworkObjectId);




    }

    [ClientRpc]
    void SetupNameplateRotationClientRpc(ulong clientId)
    {
        var holder = GetClientHolder(NetworkManager.Singleton.LocalClientId);
        GetClientPlayer(clientId).GetComponent<PlayerInterfaceManager>().localHolder = holder.transform;
        GetClientPlayer(clientId).GetComponent<PlayerInterfaceManager>().nameCard.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerNames.Instance.playerNames[clientId];



    }

    [ClientRpc]
    void EnableCameraComponentsClientRpc(ulong holderId, ulong playerId , ClientRpcParams clientRpcParams)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(holderId, out var holder))
        {
            holder.transform.GetComponent<Camera>().enabled = true;
            holder.transform.GetComponent<AudioListener>().enabled = true;
        }

        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var player))
        {
            player.GetComponent<InventoryManager>().inventory.transform.parent.gameObject.SetActive(true);
        }
    }


    [ClientRpc]
    void EnableEyesClientRpc(ulong player)
    {

    }



    [ClientRpc]
    void SpawnPlayerClientRpc(ulong clientId, ulong playerObjId, ulong holderObjId)
    {

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerObjId, out var player))
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(holderObjId, out var holder))
            {

                switch (clientId)
                {
                    case 0:
                        if (client1Player == null)
                        {
                            client1Player = player.gameObject;
                            client1Holder = holder.gameObject;
                        }
                        break;
                    case 1:
                        if (client2Player == null)
                        {
                            client2Player = player.gameObject;
                            client2Holder = holder.gameObject;
                        }
                        break;
                    case 2:
                        if(client3Player == null)
                        {
                            client3Player = player.gameObject;
                            client3Holder = holder.gameObject;
                        }
                        break;
                    case 3:
                        if(client4Player == null)
                        {
                            client4Player = player.gameObject;
                            client4Holder = holder.gameObject;
                        }
                        break;
                }

                if(clientId != NetworkManager.Singleton.LocalClientId)
                {
                    player.GetComponent<PlayerInterfaceManager>().eyes.layer = 7;
                    holder.transform.GetChild(0).gameObject.SetActive(false);
                }

                player.name = "Player" + (clientId + 1).ToString();
                player.GetComponent<PlayerInterfaceManager>().holder = holder.gameObject;
                player.GetComponent<PlayerLook>().cam = holder.transform;
                holder.GetComponent<MoveCamera>().camPos = player.GetComponent<PlayerInterfaceManager>().camPos;
                holder.name = "Player" + (clientId + 1).ToString() + "Holder";
                player.GetComponent<PlayerLook>().player = player.gameObject;
                player.GetComponent<PlayerLook>().orientation = player.GetComponent<PlayerMovement>().orientation;
                player.GetComponent<PlayerLook>().Assign();

            }

        }


        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            // Local Client is Player 1, setup other player objects for local client
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2NetId.Value, out var player2))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2HolderId.Value, out var player2Holder))
                {
                    player2.name = "Player2";
                    player2.GetComponent<PlayerLook>().enabled = false;
                    player2Holder.name = "Player2Holder";
                    player2Holder.GetComponent<MoveCamera>().camPos = player2.GetComponent<PlayerInterfaceManager>().camPos;
                    player2.GetComponent<InventoryManager>().enabled = false;
                    
                }
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player3NetId.Value, out var player3))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player3HolderId.Value, out var player3Holder))
                {
                    player3.name = "Player3";
                    player3.GetComponent<PlayerLook>().enabled = false;
                    player3Holder.name = "Player3Holder";
                    player3Holder.GetComponent<MoveCamera>().camPos = player3.GetComponent<PlayerInterfaceManager>().camPos;
                    player3.GetComponent<InventoryManager>().enabled = false;

                }
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player4NetId.Value, out var player4))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player4HolderId.Value, out var player4Holder))
                {
                    player4.name = "Player4";
                    player4.GetComponent<PlayerLook>().enabled = false;
                    player4Holder.name = "Player4Holder";
                    player4Holder.GetComponent<MoveCamera>().camPos = player4.GetComponent<PlayerInterfaceManager>().camPos;
                    player4.GetComponent<InventoryManager>().enabled = false;

                }
            }
        }
        else if (NetworkManager.Singleton.LocalClientId == 1)
        {
            // Local Client is Player 2, setup other player objects for local client
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1NetId.Value, out var player1))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1HolderId.Value, out var player1Holder))
                {
                    player1.name = "Player1";
                    player1.GetComponent<PlayerLook>().enabled = false;
                    player1Holder.name = "Player1Holder";
                    player1Holder.GetComponent<MoveCamera>().camPos = player1.GetComponent<PlayerInterfaceManager>().camPos;
                    player1.GetComponent<InventoryManager>().enabled = false;
                }
            }

            else if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player3NetId.Value, out var player3))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player3HolderId.Value, out var player3Holder))
                {
                    player3.name = "Player3";
                    player3.GetComponent<PlayerLook>().enabled = false;
                    player3Holder.name = "Player3Holder";
                    player3Holder.GetComponent<MoveCamera>().camPos = player3.GetComponent<PlayerInterfaceManager>().camPos;
                    player3.GetComponent<InventoryManager>().enabled = false;
                }
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player4NetId.Value, out var player4))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player4HolderId.Value, out var player4Holder))
                {
                    player4.name = "Player4";
                    player4.GetComponent<PlayerLook>().enabled = false;
                    player4Holder.name = "Player4Holder";
                    player4Holder.GetComponent<MoveCamera>().camPos = player4.GetComponent<PlayerInterfaceManager>().camPos;
                    player4.GetComponent<InventoryManager>().enabled = false;
                }
            }
        }
        else if (NetworkManager.Singleton.LocalClientId == 2)
        {
            // Local Client is Player 3, setup other player objects for local client
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1NetId.Value, out var player1))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1HolderId.Value, out var player1Holder))
                {

                    player1.name = "Player1";
                    player1.GetComponent<PlayerLook>().enabled = false;
                    player1Holder.name = "Player1Holder";
                    player1Holder.GetComponent<MoveCamera>().camPos = player1.GetComponent<PlayerInterfaceManager>().camPos;
                    player1.GetComponent<InventoryManager>().enabled = false;
                }
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2NetId.Value, out var player2))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2HolderId.Value, out var player2Holder))
                {

                    player2.name = "Player2";
                    player2.GetComponent<PlayerLook>().enabled = false;
                    player2Holder.name = "Player2Holder";
                    player2Holder.GetComponent<MoveCamera>().camPos = player2.GetComponent<PlayerInterfaceManager>().camPos;
                    player2.GetComponent<InventoryManager>().enabled = false;
                }
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player4NetId.Value, out var player4))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player4HolderId.Value, out var player4Holder))
                {
                    player4.name = "Player4";
                    player4.GetComponent<PlayerLook>().enabled = false;
                    player4Holder.name = "Player4Holder";
                    player4Holder.GetComponent<MoveCamera>().camPos = player4.GetComponent<PlayerInterfaceManager>().camPos;
                    player4.GetComponent<InventoryManager>().enabled = false;
                }
            }
        }
        else if (NetworkManager.Singleton.LocalClientId == 3)
        {
            // Local Client is Player 4, setup other player objects for local client
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1NetId.Value, out var player1))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player1HolderId.Value, out var player1Holder))
                {
                    player1.name = "Player1";
                    player1.GetComponent<PlayerLook>().enabled = false;
                    player1Holder.name = "Player1Holder";
                    player1Holder.GetComponent<MoveCamera>().camPos = player1.GetComponent<PlayerInterfaceManager>().camPos;
                    player1.GetComponent<InventoryManager>().enabled = false;
                }
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2NetId.Value, out var player2))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player2HolderId.Value, out var player2Holder))
                {
                    player2.name = "Player2";
                    player2.GetComponent<PlayerLook>().enabled = false;
                    player2Holder.name = "Player2Holder";
                    player2Holder.GetComponent<MoveCamera>().camPos = player2.GetComponent<PlayerInterfaceManager>().camPos;
                    player2.GetComponent<InventoryManager>().enabled = false;
                }
            }

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player3NetId.Value, out var player3))
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(player3HolderId.Value, out var player3Holder))
                {
                    player3.name = "Player3";
                    player3.GetComponent<PlayerLook>().enabled = false;
                    player3Holder.name = "Player3Holder";
                    player3Holder.GetComponent<MoveCamera>().camPos = player3.GetComponent<PlayerInterfaceManager>().camPos;
                    player3.GetComponent<InventoryManager>().enabled = false;
                }
            }
        }

        spawnedPlayers++;
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
                return client3Player;
            case 3:
                return client4Player;
            default:
                break;
        }
        return null;
    }

    public string GetClientName(ulong id)
    {
        return PlayerNames.Instance.playerNames[id];
    }

    public GameObject GetClientHolder(ulong id)
    {
        switch (id)
        {
            case 0:
                return client1Holder;
            case 1:
                return client2Holder;
            case 2:
                return client3Holder;
            case 3:
                return client4Holder;
            default:
                break;
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            if(spawnedPlayers >= NetworkManager.Singleton.ConnectedClients.Count)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClients)
                {
                    SetupNameplateRotationClientRpc(client.Key);
                }
                spawnedPlayers = 0;
            }
        }

    }
}
