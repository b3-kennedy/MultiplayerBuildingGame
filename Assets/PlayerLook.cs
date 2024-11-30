using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{

    public float sensitivity;
    public GameObject player;
    Transform playerObj;
    public Transform orientation;
    public Transform cam;

    float xRot;
    float yRot;

    // Start is called before the first frame update
    void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;

    }

    public void Assign()
    {
        playerObj = player.transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {

        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        yRot += mouseX;
        xRot -= mouseY;

        xRot = Mathf.Clamp(xRot, - 90f, 90f);

        if(cam != null)
        {
            cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        }
        
        orientation.rotation = Quaternion.Euler(0, yRot,0);
        playerObj.rotation = orientation.rotation;
    }
}
