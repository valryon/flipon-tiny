using UnityEngine;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Inventory playerInventory;

    private const string INVENTORY_KEY = "PlayerInventory";

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadInventory();
    }

    // Save the inventory to PlayerPrefs
    public void SaveInventory()
    {
        string json = JsonUtility.ToJson(playerInventory);
        PlayerPrefs.SetString(INVENTORY_KEY, json);
        PlayerPrefs.Save();
    }

    // Load the inventory from PlayerPrefs
    public void LoadInventory()
    {
        if (PlayerPrefs.HasKey(INVENTORY_KEY))
        {
            string json = PlayerPrefs.GetString(INVENTORY_KEY);
            playerInventory = JsonUtility.FromJson<Inventory>(json);
        }
        else
        {
            playerInventory = new Inventory();
        }
    }

    public bool AddItemFromShop(Item item)
    {
        if (item != null && !item.isPurchased)
        {
            item.isPurchased = true;
            SaveInventory();
            return true;  // Successfully purchased
        }
        return false;  // Item not found or already purchased
    }

    public void EnableItem(string name)
    {
        var item = playerInventory.GetItemByName(name);
        if (item != null)
        {
            item.isEnabled = true;
            SaveInventory();
        }
    }

    public void DisableItem(string name)
    {
        var item = playerInventory.GetItemByName(name);
        if (item != null)
        {
            item.isEnabled = false;
            SaveInventory();
        }
    }

    public bool HasItem(string name)
    {
        return playerInventory.HasItem(name);
    }

    public bool HasItem<T>() where T : Item
    {
        return playerInventory.HasItem<T>();
    }
}
