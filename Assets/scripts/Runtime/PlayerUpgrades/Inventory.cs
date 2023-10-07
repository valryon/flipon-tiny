using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    public void AddItem(Item item)
    {
        items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public Item GetItemByName(string name)
    {
        return items.FirstOrDefault(i => i.itemName == name);
    }

    public List<Upgrade> GetUpgrades()
    {
        return items.OfType<Upgrade>().ToList();
    }

    public List<Power> GetPowers()
    {
        return items.OfType<Power>().ToList();
    }

    public List<Incremental> GetIncrementals()
    {
        return items.OfType<Incremental>().ToList();
    }

    public bool HasItem(string name)
    {
        return items.Any(i => i.itemName == name);
    }

    public bool HasItem<T>() where T : Item
    {
        return items.OfType<T>().Any();
    }

}
