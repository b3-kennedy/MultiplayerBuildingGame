using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item
{
    public Item item;
    public Vector3 holdPos;
    public Vector3 holdRot;
    [HideInInspector] public GameObject player;

    private void Start()
    {
    }

    public virtual void Use() { }
}
