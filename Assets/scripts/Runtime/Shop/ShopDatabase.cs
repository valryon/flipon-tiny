using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    public List<Item> shopItems;
}
