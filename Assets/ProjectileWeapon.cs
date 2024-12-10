using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class ProjectileWeapon : Weapon
{
    GameObject player;
    Transform holder;
    InventoryManager inventoryManager;
    Vector3 firePoint;
    public float force = 25f;
    ItemSlot ammoSlot;
    List<GameObject> spawnedProjectiles = new List<GameObject>();
    int arrowNumber;

    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(9, 9, true);
        player = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
        holder = PlayerManager.Instance.GetClientHolder(NetworkManager.Singleton.LocalClientId).transform;
        inventoryManager = player.GetComponent<InventoryManager>();
        


    }

    public bool FindAmmo()
    {
        foreach (var slot in inventoryManager.visibleToolbeltSlots)
        {
            var itemSlot = slot.GetComponent<ItemSlot>();
            if(itemSlot.item == ammo)
            {
                ammoSlot = itemSlot;
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (gameObject.activeSelf)
        {
            
            if (Input.GetButtonDown("Fire1") && FindAmmo())
            {
                
                firePoint = holder.position + (holder.forward);
                Vector3 angle = holder.eulerAngles + new Vector3(90, 0, 0);
                GameObject projectile = Instantiate(ammo.itemObject, firePoint, Quaternion.Euler(angle));
                projectile.GetComponent<MeshRenderer>().enabled = true;
                projectile.GetComponent<Rigidbody>().isKinematic = false;
                spawnedProjectiles.Insert(0,projectile);
                arrowNumber++;
                if (ammo.itemObject.GetComponent<Arrow>())
                {
                    projectile.GetComponent<NetworkTransform>().enabled = true;
                    projectile.GetComponent<Arrow>().enabled = true;
                    projectile.GetComponent<Arrow>().fired = true;
                    projectile.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                    projectile.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
                projectile.GetComponent<Rigidbody>().AddForce(holder.forward * force, ForceMode.Impulse);
                SpawnProjectileOnServerRpc(NetworkManager.Singleton.LocalClientId, ammo.id, arrowNumber);
                ammoSlot.OnItemRemoved();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnProjectileOnServerRpc(ulong clientId, int projectileId, int arrowNum)
    {
        var holder = PlayerManager.Instance.GetClientHolder(clientId);
        var projectile = ItemHolder.Instance.GetItemFromId(projectileId);
        Vector3 angle = holder.transform.eulerAngles + new Vector3(90, 0, 0);
        GameObject spawnedProjectile = Instantiate(projectile.itemObject, holder.transform.position + holder.transform.forward, Quaternion.Euler(angle));
        spawnedProjectile.name = "ServerArrow";
        spawnedProjectile.GetComponent<NetworkObject>().Spawn();
        EnableProjectileComponentsClientRpc(spawnedProjectile.GetComponent<NetworkObject>().NetworkObjectId, arrowNum, clientId);
        Rigidbody projectileRb = spawnedProjectile.GetComponent<Rigidbody>();
        projectileRb.AddForce(holder.transform.forward * force, ForceMode.Impulse);
    }

    [ClientRpc]
    void EnableProjectileComponentsClientRpc(ulong objectId, int arrowNum, ulong clientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var proj))
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
            {
                proj.GetComponent<MeshRenderer>().enabled = true;
                proj.GetComponent<NetworkTransform>().enabled = true;
                proj.GetComponent<Rigidbody>().AddForce(PlayerManager.Instance.GetClientHolder(clientId).transform.forward * force, ForceMode.Impulse);
            }


        }
    }


}
