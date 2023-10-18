using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public ShopDatabase shopDatabase;
    public InventoryManager inventoryManager;

    private void Awake()
    {

    }

    public bool PurchaseItem(Item item)
    {
        if(item == null)
        {
            Debug.Log("item is null");
        }
        
        if (CurrencyManager.Instance.CanAfford(item.price) && !item.isPurchased)
        {
            CurrencyManager.Instance.RemoveCurrency(item.price);
            inventoryManager.AddItemFromShop(item);
            item.isPurchased = true;
            return true;  // Successful purchase
        }
        return false;  // Failed to purchase
    }
}
