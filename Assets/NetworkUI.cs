using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkUI : NetworkBehaviour
{
    public GameObject buttons;
    public GameObject playerCard;


    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);




    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();

    }


    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerCardServerRpc()
    {
        GameObject card = Instantiate(playerCard);
    }

   
}
