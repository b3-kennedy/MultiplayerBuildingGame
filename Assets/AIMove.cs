using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AIMove : MonoBehaviour
{

    public Transform target;
    NavMeshAgent agent;
    NavMeshPath path;

    // Start is called before the first frame update
    void Start()
    {
        path = new NavMeshPath();
        agent = GetComponent<NavMeshAgent>();   
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {
            agent.CalculatePath(target.position, path);
            agent.SetPath(path);
        }

    }
}
