using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InGameInventory : MonoBehaviour
{
    public Inventory playerInventory;
    public const string INVENTORY_FILENAME = "inventory.json";
    public static InGameInventory Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        LoadInventory();
    }

    public void LoadInventory()
    {
        string path = Path.Combine(Application.persistentDataPath, INVENTORY_FILENAME);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            playerInventory = JsonUtility.FromJson<Inventory>(json);
        }
        else
        {
            if (playerInventory == null)
                Debug.Log("Can't find inventory");
        }
    }

    public List<Item> GetItems()
    {
        return playerInventory.items;
    }
}
