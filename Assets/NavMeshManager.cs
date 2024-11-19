using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshManager : MonoBehaviour
{

    public static NavMeshManager Instance;

    public NavMeshSurface surface;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void BuildNavMesh()
    {
        surface.BuildNavMesh();
    }
}
