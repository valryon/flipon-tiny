using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Shop/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public int price;
    public Sprite icon;
    public bool isPurchased = false;
    public bool isEnabled = false;
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Shop/Upgrade")]
public class Upgrade : Item { }

[CreateAssetMenu(fileName = "NewPower", menuName = "Shop/Power")]
public class Power : Item { }

[CreateAssetMenu(fileName = "NewIncremental", menuName = "Shop/Incremental")]
public class Incremental : Item { }
