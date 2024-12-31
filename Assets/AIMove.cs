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

        if (agent.isOnOffMeshLink)
        {

            OffMeshLinkData linkData = agent.currentOffMeshLinkData;
            Debug.Log($"Start: {linkData.startPos}, End: {linkData.endPos}, Valid: {linkData.valid}, Activated: {linkData.activated}");
            StartCoroutine(TraverseOffMeshLink(agent));

        }
        else
        {
            if (target != null)
            {
                if (target == player)
                {

                    if (playerPath.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.CalculatePath(target.position, playerPath);
                        agent.SetPath(playerPath);
                    }
                    else if (playerPath.status != NavMeshPathStatus.PathComplete)
                    {
                        if (NavMesh.SamplePosition(target.position, out hit, Vector3.Distance(target.position, agent.transform.position), NavMesh.AllAreas))
                        {
                            agent.CalculatePath(hit.position, playerPath);
                            agent.SetPath(playerPath);
                        }

                        if (Vector3.Distance(agent.transform.position, hit.position) < 1000f)
                        {
                            Vector3 dir = target.position - transform.position;

                            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 3f, buildingLayerMask))
                            {
                                //Destroy(hit.collider.gameObject);
                            }
                        }
                    }


                }
            }
        }


    }

    IEnumerator TraverseOffMeshLink(NavMeshAgent agent)
    {
        Debug.Log("traverse");
        // Custom movement logic (e.g., jump or animation)
        Vector3 endPos = agent.currentOffMeshLinkData.endPos;
        while (Vector3.Distance(agent.transform.position, new Vector3(endPos.x, agent.transform.position.y, endPos.z)) > 0.1f)
        {
            agent.transform.position = Vector3.Lerp(agent.transform.position, new Vector3(endPos.x, agent.transform.position.y, endPos.z), agent.speed * Time.deltaTime);
            yield return null;
        }

        // Complete the link
        agent.CompleteOffMeshLink();
    }
}
