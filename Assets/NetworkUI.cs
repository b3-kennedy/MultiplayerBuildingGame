using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkUI : NetworkBehaviour
{
    public GameObject buttons;
    public GameObject playerCard;

    public TMP_InputField input;


    public void Host()
    {
        if(input.text != "")
        {
            PlayerNames.Instance.localName = input.text;
        }
        else
        {
            PlayerNames.Instance.localName = ".";
        }
        
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        



    }

    public void Join()
    {
        if (input.text != "")
        {
            PlayerNames.Instance.localName = input.text;
        }
        else
        {
            PlayerNames.Instance.localName = ".";
        }
        NetworkManager.Singleton.StartClient();
        

    }


    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerCardServerRpc()
    {
        GameObject card = Instantiate(playerCard);
    }

   
}
