using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemModal : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public Button purchaseButton;
    public TextMeshProUGUI priceText;
    GameObject itemUI;
    private Item currentItem;
    public ShopUIManager shopUIManager;
    public void Populate(Item item, GameObject openingItemUI)
    {
        currentItem = item;
        itemIcon.sprite = item.icon;
        itemName.text = item.itemName;
        itemDescription.text = item.description;
        priceText.text = item.price.ToString();
        itemUI = openingItemUI;
    }

    public void ShowModal()
    {
        this.gameObject.SetActive(true);
    }

    public void HideModal()
    {
        this.gameObject.SetActive(false);
    }

    public void Purchase()
    {
        shopUIManager.Purchase(currentItem, itemUI);
    }
}
