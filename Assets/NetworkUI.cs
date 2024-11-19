using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : NetworkBehaviour
{
    public Camera menuCamera;
    public NetworkObject player;
    public GameObject buttons;


    public void Host()
    {
        NetworkManager.Singleton.StartHost();

        // Disable menu camera and button
        menuCamera.gameObject.SetActive(false);
        buttons.SetActive(false);


    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();

        // Disable menu camera and button
        menuCamera.gameObject.SetActive(false);
        buttons.SetActive(false);

    }

   
}
