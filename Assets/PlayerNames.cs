using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNames : NetworkBehaviour
{
    // The static instance of this class
    public static PlayerNames Instance { get; private set; }


    public string localName;

    public string[] playerNames = new string[4];

    // Awake is called before Start
    private void Awake()
    {
        // Ensure that only one instance of this class exists
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate PlayerNames instance detected. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        // Set the instance and mark it as persistent
        Instance = this;
        DontDestroyOnLoad(gameObject); // Makes the GameObject persist between scenes
    }

    public int GetIndexFromName(string name)
    {
        for (int i = 0; i < playerNames.Length; i++)
        {
            if (playerNames[i].ToLower() == name.ToLower())
            {
                return i;
            }
        }
        Debug.LogWarning("Player name does not exist returning -1");
        return -1;
    }
}
