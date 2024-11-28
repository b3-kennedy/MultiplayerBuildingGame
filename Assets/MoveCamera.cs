using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform camPos;

    private void Update()
    {
        if(camPos != null)
        {
            transform.position = camPos.position;
        }
        
    }
}
