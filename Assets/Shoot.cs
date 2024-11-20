using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Shoot : NetworkBehaviour
{

    BuildingManager buildingManager;

    // Start is called before the first frame update
    void Start()
    {
        buildingManager = GetComponent<BuildingManager>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!IsOwner) return;

        if(Input.GetButtonDown("Fire1") && buildingManager.mode == BuildingManager.Mode.NORMAL)
        {
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit))
            {
                if (hit.collider.GetComponent<AIMove>())
                {
                    if (!IsServer)
                    {
                        HideZombieLocally(hit.collider.gameObject);
                    }
                    
                    HitZombieServerRpc(hit.collider.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
    }

    void HideZombieLocally(GameObject zombie)
    {
        zombie.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void HitZombieServerRpc(ulong id)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject obj))
        {
            obj.GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            Debug.Log("Object not found");
        }
    }
}
