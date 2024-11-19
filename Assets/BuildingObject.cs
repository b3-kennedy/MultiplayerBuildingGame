using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildingObject : MonoBehaviour
{

    public int id;




    public enum Status {PLACED, PLACEHOLDER};
    public Status status;
    public float yOffset;
    public float xOffset;
    public float zOffset;

    [Header("Material Cost")]
    public float wood;

    private void Start()
    {
    }
}
