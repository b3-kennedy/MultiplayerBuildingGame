using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.LowLevel;

public class AIMove : MonoBehaviour
{

    public Transform player;
    public Transform target;
    NavMeshAgent agent;
    NavMeshPath playerPath;
    NavMeshHit hit;
    public LayerMask buildingLayerMask;


    // Start is called before the first frame update
    void Start()
    {
        target = player;
        playerPath = new NavMeshPath();
        agent = GetComponent<NavMeshAgent>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (target == player)
            {

                if(playerPath.status == NavMeshPathStatus.PathComplete)
                {
                    agent.CalculatePath(target.position, playerPath);
                    agent.SetPath(playerPath);
                }
                else if(playerPath.status != NavMeshPathStatus.PathComplete)
                {
                    if(NavMesh.SamplePosition(target.position, out hit, Vector3.Distance(target.position, agent.transform.position), NavMesh.AllAreas))
                    {
                        agent.CalculatePath(hit.position, playerPath);
                        agent.SetPath(playerPath);
                    }

                    if(Vector3.Distance(agent.transform.position, hit.position) < 1000f)
                    {
                        Vector3 dir = target.position - transform.position;

                        if(Physics.Raycast(transform.position, dir, out RaycastHit hit ,3f, buildingLayerMask))
                        {
                            Destroy(hit.collider.gameObject);
                        }
                    }
                }
            }
        }
    }
}
