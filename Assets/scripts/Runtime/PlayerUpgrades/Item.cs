using UnityEngine;

[System.Serializable]
public class Item : MonoBehaviour
{
    public string itemName;
    public bool isPurchased;
    public bool isEnabled;

    public Item(string name)
    {
        itemName = name;
        isPurchased = false;
        isEnabled = false;
    }
}

[System.Serializable]
public class Upgrade : Item
{
    public Upgrade(string name) : base(name) { }
}

[System.Serializable]
public class Power : Item
{
    public Power(string name) : base(name) { }
}

[System.Serializable]
public class Incremental : Item
{
    public Incremental(string name) : base(name) { }
}
