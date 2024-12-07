using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProjectileWeapon : Weapon
{
    GameObject player;
    Transform holder;
    InventoryManager inventoryManager;
    Vector3 firePoint;
    public float force = 25f;
    ItemSlot ammoSlot;

    // Start is called before the first frame update
    void Start()
    {
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
        if (gameObject.activeSelf)
        {
            
            if (Input.GetButtonDown("Fire1") && FindAmmo())
            {

                firePoint = holder.position + (holder.forward);
                Vector3 angle = holder.eulerAngles + new Vector3(90, 0, 0);
                GameObject projectile = Instantiate(ammo.itemObject, firePoint, Quaternion.Euler(angle));
                if (ammo.itemObject.GetComponent<Arrow>())
                {
                    projectile.GetComponent<Arrow>().enabled = true;
                    projectile.GetComponent<Arrow>().fired = true;
                    projectile.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                    projectile.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
                projectile.GetComponent<Rigidbody>().AddForce(holder.forward * force, ForceMode.Impulse);
                ammoSlot.OnItemRemoved();
            }
        }
    }
}
