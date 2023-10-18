using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    public List<Power> powers;
    public List<Upgrade> upgrades;
    public List<Incremental> incrementals;

    public List<Item> GetAllItems()
    {
        List<Item> allItems = new List<Item>();
        allItems.AddRange(powers);
        allItems.AddRange(upgrades);
        allItems.AddRange(incrementals);
        return allItems;
    }

}
