using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InteractionManager : NetworkBehaviour
{

    public float interactRange;
    float treeTimer;

    InventoryManager inventoryManager;
    NetworkObject networkObject;

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();
        networkObject = GetComponent<NetworkObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;


        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, interactRange) && !inventoryManager.inventory.activeSelf)
        {
            if (hit.collider.GetComponent<Tool>())
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hit.collider.gameObject.GetComponent<ItemCount>())
                    {
                        for (int i = 0; i < hit.collider.gameObject.GetComponent<ItemCount>().itemCount.Value; i++)
                        {
                            inventoryManager.AddItem(hit.collider.GetComponent<Tool>().item);
                        }
                    }
                    else
                    {
                        inventoryManager.AddItem(hit.collider.GetComponent<Tool>().item);
                    }
                    

                    DestroyInteractObjServerRpc(hit.collider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
    }

    [ServerRpc]
    void DestroyInteractObjServerRpc(ulong objId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objId, out NetworkObject obj))
        {
            obj.Despawn();
        }
    }
}
