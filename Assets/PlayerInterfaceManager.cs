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
    public Transform camPos;
    int zombieCount = 0;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            transform.position = new Vector3(0, 2, 0);

            GameObject holder = Instantiate(cameraHolder);
            holder.GetComponent<MoveCamera>().camPos = camPos;
            GetComponent<PlayerLook>().player = gameObject;
            GetComponent<PlayerLook>().orientation = GetComponent<PlayerMovement>().orientation;
            GetComponent<PlayerLook>().Assign();
            GetComponent<PlayerLook>().cam = holder.transform;
        }

        if (!IsOwner)
        {
            eyes.layer = 7;
            hud.SetActive(false);
            GetComponent<PlayerLook>().enabled = false;
            GetComponent<InventoryManager>().enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            
            
            SpawnZombieServerRpc();
            zombieCount++;
            zombieCounter.text = "ZOMBIE COUNTER: " + zombieCount.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnZombieServerRpc()
    {
        GameObject e = Instantiate(enemy);
        e.GetComponent<AIMove>().player = transform;
        e.GetComponent<NetworkObject>().Spawn();
    }


}
