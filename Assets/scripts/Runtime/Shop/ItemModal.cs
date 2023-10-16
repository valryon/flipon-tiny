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

    public void Populate(Item item)
    {
        itemIcon.sprite = item.icon;
        itemName.text = item.itemName;
        itemDescription.text = item.description;
        priceText.text = item.price.ToString();
    }

    public void ShowModal()
    {
        this.gameObject.SetActive(true);
    }

    public void HideModal()
    {
        this.gameObject.SetActive(false);
    }
}
