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
            if (hit.collider.CompareTag("Tree"))
            {
                InteractWithTree(hit.collider.gameObject);
            }
        }
    }

    void InteractWithTree(GameObject tree)
    {
        if (Input.GetButton("Fire1"))
        {
            treeTimer += Time.deltaTime;
            if(treeTimer >= 0.5f)
            {
                if (!IsServer)
                {
                    tree.GetComponent<Tree>().TakeDamageLocally(20);
                }
                tree.GetComponent<Tree>().TakeDamageServerRpc(20, OwnerClientId);
                treeTimer = 0;
            }
        }
    }
}
