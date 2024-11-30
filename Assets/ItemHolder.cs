using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public class ItemObjectAndId
{
    public GameObject itemObject;
    [HideInInspector] public int id;
}

public class ItemHolder : MonoBehaviour
{
    public static ItemHolder Instance;

    public Item wood;
    public Item stone;

    [Header("Item Objects")]
    public ItemObjectAndId[] objects;

    int index = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        foreach (var item in objects)
        {
            item.itemObject.GetComponent<Item>().id = index;
            item.id = index;
            index++;
        }
    }

    public int GetItemIdFromGameObject(GameObject obj)
    {
        
        foreach (var item in objects)
        {
            Debug.Log(item.itemObject);
            Debug.Log(obj);
            if (item.itemObject == obj)
            {
                return item.id;
            }
        }
        return -1;
    }

    public GameObject GetItemObjectFromId(int id)
    {
        foreach (var item in objects)
        {
            if (item.id == id)
            {
                return item.itemObject;
            }
        }
        return null;
    }





}
