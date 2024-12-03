using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Axe : Tool
{

    float treeTimer;

    public override void Use()
    {

    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 5f))
            {
                if (hit.collider.CompareTag("Tree"))
                {
                    InteractWithTree(hit.collider.gameObject);
                }
            }
        }
    }

    void InteractWithTree(GameObject tree)
    {
        if (Input.GetButton("Fire1"))
        {
            treeTimer += Time.deltaTime;
            if (treeTimer >= 0.5f)
            {
                if (!IsServer)
                {
                    tree.GetComponent<Tree>().TakeDamageLocally(20);
                }

                tree.GetComponent<Tree>().TakeDamageServerRpc(20, NetworkManager.Singleton.LocalClientId);
                treeTimer = 0;
            }
        }
    }
}
