using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FirstPersonMovement : NetworkBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody rb;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true;
        }
               
    }

    IEnumerator WaitForFrame()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("BEFORE: " + gameObject.GetComponent<NetworkObject>().OwnerClientId);
        
        Debug.Log("AFTER: " + gameObject.GetComponent<NetworkObject>().OwnerClientId);



    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeOwnerServerRpc(ulong objectId, ulong clientId)
    {
        // Check if the NetworkObject exists in the spawned objects dictionary
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            networkObject.ChangeOwnership(clientId);
            
        }
        else
        {
            Debug.Log("Object Not Found");
        }
    }

    void Awake()
    {
        // Get the rigidbody on this.
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {


        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity =new Vector2( Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);
        rb.velocity = transform.rotation * new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.y);
        // Apply movement.

    }
}