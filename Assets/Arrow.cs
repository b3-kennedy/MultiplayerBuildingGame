using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

    Rigidbody rb;
    Vector3 vel;
    [HideInInspector]public bool fired = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        vel = rb.velocity;
        if(vel.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(vel.normalized);

            targetRot *= Quaternion.Euler(90, 0, 0);

            transform.rotation = targetRot;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (fired)
        {
            rb.isKinematic = true;
        }
        
    }
}
