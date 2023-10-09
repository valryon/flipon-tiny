using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public ShopDatabase shopDatabase;
    public GameObject shopItemPrefab;  // This prefab will represent each item in the shop UI
    public Transform shopContainer;    // The parent object where all items will be displayed

    public Transform upgradesRowTransform;
    public Transform powersRowTransform;
    public Transform incrementalsRowTransform;

    private void Start()
    {
        PopulateShop();
    }

    public void PopulateShop()
    {
        foreach (var item in shopDatabase.shopItems)
        {
            GameObject itemUI;

            if (item is Upgrade)
            {
                itemUI = Instantiate(shopItemPrefab, upgradesRowTransform);
            }
            else if (item is Power)
            {
                itemUI = Instantiate(shopItemPrefab, powersRowTransform);
            }
            else // Assumes item is Incremental or any other type
            {
                itemUI = Instantiate(shopItemPrefab, incrementalsRowTransform);
            }
            itemUI.transform.Find("Name").GetComponent<Text>().text = item.itemName;
            itemUI.transform.Find("Price").GetComponent<Text>().text = item.price.ToString();
            itemUI.transform.Find("Description").GetComponent<Text>().text = item.description;
            itemUI.transform.Find("Icon").GetComponent<Image>().sprite = item.icon;

            Button purchaseButton = itemUI.transform.Find("PurchaseButton").GetComponent<Button>();
            purchaseButton.onClick.AddListener(() => ShopManager.Instance.PurchaseItem(item));

            if (item.isPurchased)
            {
                // Optionally, update the UI to reflect that the item is purchased
                purchaseButton.interactable = false;
            }
        }
    }
}
