using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemCount : NetworkBehaviour
{
    public NetworkVariable<int> itemCount = new NetworkVariable<int>(1);

    private void Start()
    {
    }
}
