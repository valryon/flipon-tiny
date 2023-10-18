using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.IO;

public class InventoryManager : MonoBehaviour
{

    public Inventory playerInventory;

    public GameObject inventoryCanvas;
    public GameObject itemContainerPrefab;
    public Transform itemsParentTransform;

    private void Awake()
    {
        LoadInventory();
        PopulateInventoryUI();
    }

    public const string INVENTORY_FILENAME = "inventory.json";

    public void SaveInventory()
    {
        string path = Path.Combine(Application.persistentDataPath, INVENTORY_FILENAME);
        string json = JsonUtility.ToJson(playerInventory);
        File.WriteAllText(path, json);
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
            if(playerInventory == null)
                playerInventory = new Inventory();
        }
    }


    public bool AddItemFromShop(Item item)
    {
        if (item != null && !item.isPurchased)
        {
            playerInventory.AddItem(item);
            EnableItem(item);
            item.isPurchased = true;
            Debug.Log($"Adding {item.itemName} to inventory.");
            SaveInventory();

            PopulateInventoryUI();

            return true;  // Successfully purchased
        }
        return false;  // Item not found or already purchased
    }

    public void EnableItem(Item item)
    {
        if (item != null)
        {
            item.isEnabled = true;
            SaveInventory();
        }
    }

    public void DisableItem(Item item)
    {
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

    public void PopulateInventoryUI()
    {
        Debug.Log("Populating inventory UI.");
        // First, remove all previously spawned items.
        foreach (Transform child in itemsParentTransform)
        {
            Destroy(child.gameObject);
        }

        // Now, instantiate new items based on the player's inventory.
        foreach (var item in playerInventory.items)
        {
            Debug.Log($"Instantiating UI for item: {item.itemName}");

            var itemUI = Instantiate(itemContainerPrefab, itemsParentTransform);
            itemUI.transform.Find("ItemIcon").GetComponent<Image>().sprite = item.icon;

            // Set the enabled state
            itemUI.transform.Find("EnabledPanel").gameObject.SetActive(item.isEnabled);

            // Add the click listener to toggle the enabled state.
            Button itemButton = itemUI.transform.Find("EnableTap").GetComponent<Button>();
            itemButton.onClick.AddListener(() => ToggleItemEnabled(item, itemUI));
        }
    }

    public void ToggleItemEnabled(Item item, GameObject itemUI)
    {
        if (item.isEnabled)
        {
            DisableItem(item);
            itemUI.transform.Find("EnabledPanel").gameObject.SetActive(false);
        }
        else
        {
            EnableItem(item);
            itemUI.transform.Find("EnabledPanel").gameObject.SetActive(true);
        }
    }

}
