using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class ServerSlot : NetworkBehaviour
{
    public NetworkVariable<bool> hasItem = new NetworkVariable<bool>(false);
    public NetworkVariable<int> itemId = new NetworkVariable<int>(0);
    public NetworkVariable<int> itemCount = new NetworkVariable<int>(0);


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
