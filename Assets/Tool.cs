using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item
{
    public Item item;
    public Vector3 holdPos;
    public Vector3 holdRot;
    public GameObject player;

    private void Start()
    {
        if(item != null)
        {
            maxStackCount = item.maxStackCount;
            isStackable = item.isStackable;
        }
    }

    public virtual void Use() { }
}
