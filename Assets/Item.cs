using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Item : NetworkBehaviour
{
    public int maxStackCount;
    public bool isStackable = false;
    public GameObject icon;
    public GameObject itemObject;
}
