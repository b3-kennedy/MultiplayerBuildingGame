using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Pickaxe : Tool
{

    float rockTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 5f))
            {
                if (hit.collider.CompareTag("Rock"))
                {
                    InteractWithRock(hit.collider.gameObject);
                }
            }
        }
    }

    void InteractWithRock(GameObject rock)
    {
        if (Input.GetButton("Fire1"))
        {
            rockTimer += Time.deltaTime;
            if (rockTimer >= 0.5f)
            {
                if (!IsServer)
                {
                    rock.GetComponent<RockObject>().TakeDamageLocally(20);
                }

                rock.GetComponent<RockObject>().TakeDamageServerRpc(20, NetworkManager.Singleton.LocalClientId);
                rockTimer = 0;
            }
        }
    }
}
