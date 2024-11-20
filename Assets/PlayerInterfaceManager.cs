using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerInterfaceManager : NetworkBehaviour
{
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI zombieCounter;
    public FirstPersonAudio playerAudio;
    public GameObject cam;
    public GameObject hud;
    public GameObject enemy;
    public GameObject eyes;
    int zombieCount = 0;

    public override void OnNetworkSpawn()
    {



        if (!IsOwner)
        {
            eyes.layer = 7;
            cam.SetActive(false);
            GetComponent<FirstPersonMovement>().enabled = false;
            playerAudio.gameObject.SetActive(false);
            hud.SetActive(false);
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
        e.GetComponent<AIMove>().target = transform;
        e.GetComponent<NetworkObject>().Spawn();
    }


}
