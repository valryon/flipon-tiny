using UnityEngine;
public enum ItemType
{
    Upgrade,
    Power,
    Incremental
}

[CreateAssetMenu(menuName = "Shop/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public int price;
    public Sprite icon;
    public bool isPurchased = false;
    public bool isEnabled = false;
    public ItemType itemType;
    public string itemCodeName;
    public int incLevel;
    public int incMaxLevel;
}

