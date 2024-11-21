using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float speed;

    public float groundDrag;

    public Transform groundCheck;

    public Transform orientation;

    public float jumpForce;
    public float airMultiplier;

    float horizontal;
    float vertical;

    Vector3 moveDir;

    Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    //public override void OnNetworkSpawn()
    //{
    //    if (IsOwner)
    //    {
    //        rb.isKinematic = false;
    //    }
    //    else
    //    {
    //        rb.isKinematic = true;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;


        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        SpeedControl();

        if (IsGrounded())
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, -transform.up, 0.5f);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        moveDir = orientation.forward * vertical + orientation.right * horizontal;

        if (IsGrounded())
        {
            rb.AddForce(moveDir.normalized * speed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDir.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }
        
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if(flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}
