using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class BuildObject
{
    public GameObject obj;
    public Material material;
}

public class BuildingManager : NetworkBehaviour
{

    public enum Mode {BUILD, NORMAL, DESTROY}

    public Mode mode;

    public float gridSizeX;
    public float gridSizeY;
    public float gridSizeZ;

    public float chestGridSize;

    public GameObject floor;
    public GameObject wall;
    public GameObject ramp;
    public GameObject windowWall;
    public GameObject chest;

    public GameObject buildObject;
    [HideInInspector] public GameObject currentObject;
    public LayerMask layer;
    public LayerMask destroyLayer;

    GameObject destroyObject;
    Material destroyNormalMat;

    public Material placeholderMat;
    public Material destroyMat;
    Material normalMat;

    string buildText = "MODE: BUILD";
    string normalText = "MODE: NORMAL";
    string destroyText = "MODE: DESTROY";

    PlayerInterfaceManager playerInterfaceManager;
    InventoryManager inventoryManager;

    List<GameObject> recentlyPlaced = new List<GameObject>();

    int index;

    public BuildObject[] buildObjects;

    int buildIndex;

    Vector3 gridPos;


    // Start is called before the first frame update
    void Start()
    {
        mode = Mode.NORMAL;
        playerInterfaceManager = GetComponent<PlayerInterfaceManager>();
        inventoryManager = GetComponent<InventoryManager>();
        buildObject = floor;
    }

    void ChangeMaterialInChildren(Transform obj, Material mat)
    {
        if (obj.GetComponent<MeshRenderer>())
        {
            obj.GetComponent<MeshRenderer>().material = mat;
        }

        if(obj.childCount > 0)
        {
            for(int i = 0; i < obj.childCount; i++)
            {
                if (obj.transform.GetChild(i).GetComponent<MeshRenderer>())
                {
                    obj.transform.GetChild(i).GetComponent<MeshRenderer>().material = mat;
                }
                
            }
        }
    }

    void BuildMode()
    {
        if (Input.GetButtonDown("Fire3"))
        {
            mode = Mode.DESTROY;

            Destroy(currentObject);

        }

        // Switching between floor and wall objects
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Destroy(currentObject);
            buildObject = floor; // Set the buildObject to floor
            buildIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Destroy(currentObject);
            buildObject = wall; // Set the buildObject to wall
            buildIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Destroy(currentObject);
            buildObject = ramp;
            buildIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Destroy(currentObject);
            buildObject = windowWall;
            buildIndex = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Destroy(currentObject);
            buildObject = chest;
            buildIndex = 4;
        }

        // Perform raycasting to detect where the object should snap
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 10, layer))
        {
            // Check if we hit a wall or another target for contextual snapping
            if (hit.collider.CompareTag("Wall"))
            {
                // Dynamically adjust the Y grid size based on the wall's height
                float wallHeight = hit.collider.transform.localScale.y; // Get the wall's height

                // Snap position to align with the top of the wall
                gridPos = new Vector3(
                    Mathf.Round(hit.point.x / gridSizeX) * gridSizeX,          // Snap X
                    Mathf.Round(hit.point.y / 4) * 4,          // Snap Y
                    Mathf.Round(hit.point.z / gridSizeZ) * gridSizeZ           // Snap Z
                );

                if (buildObject == floor)
                {
                    gridPos += hit.normal * (buildObject.transform.localScale.x / 2);
                }


            }
            else if (hit.collider.CompareTag("Floor"))
            {
                // Snap position to align with the top of the wall
                gridPos = new Vector3(
                    Mathf.Round(hit.point.x / gridSizeX) * gridSizeX,          // Snap X
                    Mathf.Round(hit.point.y / gridSizeY) * gridSizeY,          // Snap Y
                    Mathf.Round(hit.point.z / gridSizeZ) * gridSizeZ           // Snap Z
                );

                if (buildObject == floor)
                {
                    gridPos += hit.normal * (buildObject.transform.localScale.x / 2);
                }
                else if (buildObject == chest)
                {
                    gridPos = new Vector3(Mathf.Round(hit.point.x / (gridSizeX / chestGridSize)) * (gridSizeX / chestGridSize), (Mathf.Round(hit.point.y / gridSizeY) * gridSizeY) + 0.2f, Mathf.Round(hit.point.z / (gridSizeZ / chestGridSize)) * (gridSizeZ / chestGridSize)); ;
                }
            }
            else
            {
                // Default snapping logic
                if (currentObject != null)
                {
                    if (!currentObject.GetComponent<Chest>())
                    {
                        gridPos = new Vector3(Mathf.Round(hit.point.x / gridSizeX) * gridSizeX, Mathf.Round(hit.point.y / gridSizeY) * gridSizeY, Mathf.Round(hit.point.z / gridSizeZ) * gridSizeZ);
                    }
                    else if (currentObject.GetComponent<Chest>())
                    {
                        gridPos = new Vector3(Mathf.Round(hit.point.x / (gridSizeX / chestGridSize)) * (gridSizeX / chestGridSize), Mathf.Round(hit.point.y / gridSizeY) * gridSizeY, Mathf.Round(hit.point.z / (gridSizeZ / chestGridSize)) * (gridSizeZ / chestGridSize));
                    }
                }


            }

            // Handle the current placeholder object
            if (currentObject == null)
            {
                // Create the placeholder object
                currentObject = Instantiate(buildObject, gridPos, buildObject.transform.rotation);
                normalMat = currentObject.GetComponent<MeshRenderer>().material;
                currentObject.GetComponent<Collider>().enabled = false;
                ChangeMaterialInChildren(currentObject.transform, placeholderMat);
                currentObject.GetComponent<BuildingObject>().status = BuildingObject.Status.PLACEHOLDER;
            }
            else
            {
                // Update the position of the placeholder
                currentObject.transform.position = new Vector3(
                    gridPos.x,
                    gridPos.y + currentObject.GetComponent<BuildingObject>().yOffset, // Apply Y offset for alignment
                    gridPos.z
                );
            }
        }

        // Handle additional interactions with the placeholder object
        if (currentObject != null)
        {
            // Rotate the placeholder object
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentObject.transform.eulerAngles = new Vector3(
                    currentObject.transform.eulerAngles.x,
                    (currentObject.transform.eulerAngles.y + 90) % 360,
                    currentObject.transform.eulerAngles.z
                );
            }

            if (CheckMaterialRequirement())
            {
                if (Input.GetButtonDown("Fire1") && !inventoryManager.inventory.activeSelf)
                {

                    if (!IsServer)
                    {
                        BuildLocally(buildIndex);
                    }

                    // Instantiate a placed version of the object
                    BuildOnServerRpc(currentObject.transform.position.x, currentObject.transform.position.y, currentObject.transform.position.z,
                        currentObject.transform.eulerAngles.x, currentObject.transform.eulerAngles.y, currentObject.transform.eulerAngles.z,
                        buildIndex, OwnerClientId);

                    for (int i = 0; i < currentObject.GetComponent<BuildingObject>().wood; i++)
                    {
                        inventoryManager.RemoveItem(ItemHolder.Instance.wood);
                        inventoryManager.woodCount--;
                    }
                    Destroy(currentObject);

                }
            }
            else
            {
                ChangeMaterialInChildren(currentObject.transform, destroyMat);
            }
            // Place the object when the user clicks

        }
    }

    bool CheckMaterialRequirement()
    {
        return inventoryManager.woodCount >= currentObject.GetComponent<BuildingObject>().wood;
    }

    void BuildLocally(int buildI)
    {
        GameObject placed = Instantiate(buildObjects[buildI].obj, currentObject.transform.position, currentObject.transform.rotation);
        recentlyPlaced.Insert(0, placed);
        placed.GetComponent<Collider>().enabled = true;
        placed.GetComponent<BuildingObject>().status = BuildingObject.Status.PLACED;
        placed.GetComponent<BuildingObject>().id = index;
        index++;
        if (placed.GetComponent<MeshRenderer>())
        {
            placed.GetComponent<MeshRenderer>().material = currentObject.GetComponent<MeshRenderer>().material;
        }

        if (placed.GetComponent<MeshRenderer>())
        {
            placed.GetComponent<MeshRenderer>().material = buildObjects[buildI].material;
        }

        if (placed.transform.childCount > 0)
        {
            for (int i = 0; i < placed.transform.childCount; i++)
            {
                if (placed.transform.GetChild(i).GetComponent<MeshRenderer>())
                {
                    placed.transform.GetChild(i).GetComponent<MeshRenderer>().material = buildObjects[buildI].material;
                }

            }
        }
        placed.layer = 3;
        NavMeshManager.Instance.BuildNavMesh();
    }

    [ClientRpc]
    void BuildCompleteOnServerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if(recentlyPlaced.Count > 0)
        {
            Destroy(recentlyPlaced[recentlyPlaced.Count - 1]);
            recentlyPlaced.RemoveAt(recentlyPlaced.Count - 1);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    void BuildOnServerRpc(float posX, float posY, float posZ, float rotX, float rotY , float rotZ, int buildI, ulong clientId)
    {
        GameObject placed = Instantiate(buildObjects[buildI].obj, new Vector3(posX, posY, posZ), Quaternion.Euler(rotX, rotY, rotZ));
        placed.GetComponent<Collider>().enabled = true;
        placed.GetComponent<BuildingObject>().status = BuildingObject.Status.PLACED;
        placed.GetComponent<BuildingObject>().id = index;
        index++;
        if (placed.GetComponent<MeshRenderer>())
        {
            placed.GetComponent<MeshRenderer>().material = buildObjects[buildI].material;
        }

        if (placed.transform.childCount > 0)
        {
            for (int i = 0; i < placed.transform.childCount; i++)
            {
                if (placed.transform.GetChild(i).GetComponent<MeshRenderer>())
                {
                    placed.transform.GetChild(i).GetComponent<MeshRenderer>().material = buildObjects[buildI].material;
                }
                
            }
        }
        placed.layer = 3;
        placed.GetComponent<NetworkObject>().Spawn();
        UpdateObjectLayerClientRpc(placed.GetComponent<NetworkObject>().NetworkObjectId);
        NavMeshManager.Instance.BuildNavMesh();

        ClientRpcParams rpcParams = new ClientRpcParams 
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };
        BuildCompleteOnServerClientRpc(rpcParams);

    }

    [ClientRpc]
    void UpdateObjectLayerClientRpc(ulong objectId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject obj))
        {
            obj.gameObject.layer = 3;
        }
        else
        {
            Debug.Log("Object not found");
        }
    }

    void DestroyMode()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 10, destroyLayer))
        {
            GameObject currentDestroyObject = hit.collider.gameObject;

            // If we're looking at a new object
            if (currentDestroyObject != destroyObject)
            {
                // Reset the material of the previous object
                if (destroyObject != null && destroyNormalMat != null)
                {
                    ChangeMaterialInChildren(destroyObject.transform, destroyNormalMat);
                }

                destroyObject = currentDestroyObject;
                destroyNormalMat = destroyObject.GetComponent<MeshRenderer>().material;
                ChangeMaterialInChildren(currentDestroyObject.transform, destroyMat);

                
                
            }
        }
        else
        {
            // Reset the material of the previously highlighted object when no longer looking at anything
            if (destroyObject != null && destroyNormalMat != null)
            {
                ChangeMaterialInChildren(destroyObject.transform, destroyNormalMat);
                destroyObject = null; // Clear the reference
                destroyNormalMat = null;
            }
        }

        if (Input.GetButtonDown("Fire3"))
        {
            if (destroyObject != null && destroyNormalMat != null)
            {
                ChangeMaterialInChildren(destroyObject.transform, destroyNormalMat);
                destroyObject = null; // Clear the reference
                destroyNormalMat = null;
            }
            mode = Mode.BUILD;
        }

        if (Input.GetButtonDown("Fire1") && !inventoryManager.inventory.activeSelf)
        {
            if (!IsServer && hit.collider != null)
            {
                hit.collider.gameObject.SetActive(false);
            }
            DestroyOnServerRpc(hit.collider.GetComponent<NetworkObject>().NetworkObjectId);
            for (int i = 0; i < hit.collider.GetComponent<BuildingObject>().wood; i++)
            {
                inventoryManager.AddItem(ItemHolder.Instance.wood);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyOnServerRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject obj))
        {
            obj.Despawn();
        }
        else
        {
            Debug.Log("Object not found");
        }
    }

    void Update()
    {
        if (!IsOwner) return;


        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(mode == Mode.NORMAL)
            {

                mode = Mode.BUILD;
            }
            else
            {
                Destroy(currentObject);
                mode = Mode.NORMAL;
            }
        }

        if(mode == Mode.DESTROY)
        {
            if (playerInterfaceManager.modeText.text != destroyText)
            {
                playerInterfaceManager.modeText.text = destroyText;
            }
            DestroyMode();
        }
        else if(mode == Mode.BUILD)
        {
            if (playerInterfaceManager.modeText.text != buildText)
            {
                playerInterfaceManager.modeText.text = buildText;
            }
            BuildMode();
        }
        else if(mode == Mode.NORMAL)
        {
            if (playerInterfaceManager.modeText.text != normalText)
            {
                playerInterfaceManager.modeText.text = normalText;
            }
            if (destroyObject != null && destroyNormalMat != null)
            {
                ChangeMaterialInChildren(destroyObject.transform, destroyNormalMat);
                destroyObject = null; // Clear the reference
                destroyNormalMat = null;
            }
        }
        

    }


}
