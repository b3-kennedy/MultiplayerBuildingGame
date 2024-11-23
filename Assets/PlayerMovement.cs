using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    public enum PlayerState {NORMAL, SPRINT};
    public PlayerState state;

    

    
    public float normalSpeed;
    public float sprintSpeed;
    public float acceleration;
    public float groundDrag;

    public Transform groundCheck;

    public Transform orientation;

    public float jumpForce;
    public float airMultiplier;

    float horizontal;
    float vertical;

    

    bool normalOnLand = false;

    bool wasAirborne;

    Vector3 moveDir;
    Rigidbody rb;

    public float landingSlowdownFactor = 0.5f; // Percentage of speed after landing
    public float landingSlowdownDuration = 1.0f; // Time in seconds to recover full speed
    private float currentSlowdownTime = 0f;
    private bool isRecoveringSpeed = false;

    float targetSpeedOnLand;

    bool isSprinting = false;
    public float sprintMultiplier;
    float smoothVel;


    [Header("Debugging")]
    public float targetSpeed;
    public float speed;
    public float magnitude;



    // Start is called before the first frame update
    void Start()
    {
        speed = normalSpeed;
        targetSpeed = normalSpeed;
        state = PlayerState.NORMAL;
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

        magnitude = rb.velocity.magnitude;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
            state = PlayerState.SPRINT;
            targetSpeed = sprintSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && IsGrounded())
        {
            isSprinting = false;
            state = PlayerState.NORMAL;
            speed = normalSpeed;
            targetSpeed = normalSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && !IsGrounded())
        {
            normalOnLand = true;
        }

        if(IsGrounded() && normalOnLand)
        {
            isSprinting = false;
            state = PlayerState.NORMAL;
            speed = normalSpeed;
            targetSpeed = normalSpeed;
            normalOnLand = false;
        }



        bool grounded = IsGrounded();



        if(wasAirborne && grounded)
        {
            OnLand();
            wasAirborne = false;
        }

        if (!grounded)
        {
            wasAirborne = true;
        }

        if (isSprinting)
        {
            speed = Mathf.MoveTowards(speed, sprintSpeed, Time.deltaTime * sprintMultiplier);
        }

        // Gradually recover speed after landing
        if (isRecoveringSpeed)
        {
            currentSlowdownTime += Time.deltaTime;
            speed = Mathf.Lerp(targetSpeedOnLand * landingSlowdownFactor, targetSpeedOnLand, currentSlowdownTime / landingSlowdownDuration);

            if (currentSlowdownTime >= landingSlowdownDuration)
            {
                isRecoveringSpeed = false;
                speed = targetSpeedOnLand; // Ensure full speed is restored
            }
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

    void OnLand()
    {
        landingSlowdownFactor = (1 - (targetSpeed / 10));
        targetSpeedOnLand = targetSpeed;
        //rb.velocity = Vector3.zero;
        StartLandingSlowdown();
    }

    void StartLandingSlowdown()
    {
        isRecoveringSpeed = true;
        currentSlowdownTime = 0f;
        speed = targetSpeedOnLand * landingSlowdownFactor; // Initial slowdown
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, -transform.up, 0.51f);
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
