using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerInterfaceManager : NetworkBehaviour
{
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI zombieCounter;
    public GameObject hud;
    public GameObject enemy;
    public GameObject eyes;
    public GameObject cameraHolder;
    public GameObject serverCameraHolder;
    public GameObject holder;
    public Transform serverItemPos;
    public GameObject wall;
    public Transform camPos;

    public GameObject nameCard;

    int zombieCount = 0;

    public Transform lookAtPoint;

    public Transform localHolder;

    public override void OnNetworkSpawn()
    {

        if (IsOwner)
        {
            nameCard.SetActive(false);
            
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
    }

    void ClientConnected(ulong clientId)
    {

        
    }

    private void Update()
    {
        if(localHolder != null)
        {
            nameCard.transform.LookAt(localHolder);
        }
        
    }

    //public override void OnNetworkSpawn()
    //{


    //    if (!IsOwner)
    //    {
    //        eyes.layer = 7;
    //        hud.SetActive(false);
    //        GetComponent<PlayerLook>().enabled = false;
    //        GetComponent<InventoryManager>().enabled = false;


    //    }
    //    else
    //    {
    //        transform.position = new Vector3(0, 2, 0);

    //        SpawnHolderForLocalClient();
    //        SpawnHolderServerRpc(OwnerClientId);


    //    }
    //}

    //private void SpawnHolderForLocalClient()
    //{
    //    // This is done only for the local client
    //    holder = Instantiate(cameraHolder);
    //    holder.GetComponent<MoveCamera>().camPos = camPos; // Local camera position

    //    // Setup the camera and other components for the local client
    //    var playerLook = GetComponent<PlayerLook>();
    //    var playerMovement = GetComponent<PlayerMovement>();

    //    playerLook.player = gameObject;
    //    playerLook.orientation = playerMovement.orientation;
    //    playerLook.Assign();
    //    playerLook.cam = holder.transform;
    //}

    //[ServerRpc(RequireOwnership = false)]
    //void SpawnHolderServerRpc(ulong clientId)
    //{
    //    // Ensure we have the client that we're spawning for
    //    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
    //    {
    //        holder = Instantiate(serverCameraHolder);
    //        holder.GetComponent<MoveCamera>().camPos = client.PlayerObject.transform;
    //        holder.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    //        holder.transform.GetChild(2).GetComponent<Camera>().enabled = false;
    //        holder.transform.GetChild(2).GetComponent<AudioListener>().enabled = false;

    //        SpawnHolderClientRpc(holder.GetComponent<NetworkObject>().NetworkObjectId, 
    //            client.PlayerObject.GetComponent<NetworkObject>().NetworkObjectId, clientId);

    //    }


    //}

    //[ClientRpc]
    //void SpawnHolderClientRpc(ulong netObjId, ulong clientObjId, ulong clientId) 
    //{
    //    if(OwnerClientId == clientId)
    //    {
    //        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(clientObjId, out var clientPlayerObj))
    //        {
    //            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out var netObj))
    //            {
    //                netObj.GetComponent<MoveCamera>().camPos = clientPlayerObj.transform;
    //                netObj.transform.GetChild(2).GetComponent<Camera>().enabled = false;
    //                netObj.transform.GetChild(2).GetComponent<AudioListener>().enabled = false;
    //            }
    //        }
    //    }



    //}






    //private void Update()
    //{
    //    if (!IsOwner) return;

    //    if (Input.GetKeyDown(KeyCode.P))
    //    {


    //        SpawnZombieServerRpc();
    //        zombieCount++;
    //        zombieCounter.text = "ZOMBIE COUNTER: " + zombieCount.ToString();
    //    }
    //}

    //[ServerRpc(RequireOwnership = false)]
    //void SpawnZombieServerRpc()
    //{
    //    GameObject e = Instantiate(enemy);
    //    e.GetComponent<AIMove>().player = transform;
    //    e.GetComponent<NetworkObject>().Spawn();
    //}


}
