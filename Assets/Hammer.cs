using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : Tool
{

    BuildingManager buildingManager;

    // Start is called before the first frame update
    void Start()
    {
        if(player != null)
        {
            buildingManager = player.GetComponent<BuildingManager>();
        }
        
    }

    private void OnEnable()
    {
        if (buildingManager != null)
        {
            buildingManager.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(buildingManager != null)
        {
            if (gameObject.activeSelf && buildingManager.enabled == false)
            {
                buildingManager.enabled = true;
            }
        }

    }

    private void OnDisable()
    {
        if(buildingManager != null)
        {
            if(buildingManager.currentObject != null)
            {
                Destroy(buildingManager.currentObject);
            }
            
            buildingManager.enabled = false;
        }
    }
}
