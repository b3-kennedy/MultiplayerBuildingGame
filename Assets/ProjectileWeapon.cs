using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProjectileWeapon : Weapon
{
    GameObject player;
    Transform holder;
    Vector3 firePoint;
    public float force = 25f;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
        holder = PlayerManager.Instance.GetClientHolder(NetworkManager.Singleton.LocalClientId).transform;
        


    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            
            if (Input.GetButtonDown("Fire1"))
            {
                firePoint = holder.position + (holder.forward);
                Vector3 angle = holder.eulerAngles + new Vector3(90, 0, 0);
                GameObject projectile = Instantiate(ammo.itemObject, firePoint, Quaternion.Euler(angle));
                projectile.GetComponent<Rigidbody>().AddForce(holder.forward * force, ForceMode.Impulse);
            }
        }
    }
}
