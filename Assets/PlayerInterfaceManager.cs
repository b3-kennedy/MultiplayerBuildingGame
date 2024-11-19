using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerInterfaceManager : NetworkBehaviour
{
    public TextMeshProUGUI modeText;
    public FirstPersonAudio playerAudio;
    public GameObject cam;
    public GameObject hud;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Debug.Log("cam settings");
            cam.SetActive(false);
            GetComponent<FirstPersonMovement>().enabled = false;
            playerAudio.gameObject.SetActive(false);
            hud.SetActive(false);
        }
    }


}
