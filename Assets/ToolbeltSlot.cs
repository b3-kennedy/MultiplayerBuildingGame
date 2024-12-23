using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ToolbeltSlot : MonoBehaviour
{

    public GameObject activeItem;
    ItemSlot slot;

    // Start is called before the first frame update
    void Start()
    {
        slot = GetComponent<ItemSlot>();
    }

    // Update is called once per frame
    void Update()
    {
        if(slot.item == null && activeItem != null)
        {
            slot.inventoryManager.DestroyItemOnServerRpc(activeItem.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }


}
