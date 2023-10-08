using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<Item> availableItems;  // Assign this list in the editor with the ScriptableObjects you've created
    public static ShopManager Instance { get; private set; }

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
    }

    public bool PurchaseItem(Item item)
    {
        if (CurrencyManager.Instance.CanAfford(item.price) && !item.isPurchased)
        {
            CurrencyManager.Instance.RemoveCurrency(item.price);
            item.isPurchased = true;
            // Here, you could also add the item to the player's inventory
            InventoryManager.Instance.AddItemFromShop(item);
            return true;  // Successful purchase
        }
        return false;  // Failed to purchase
    }
}
