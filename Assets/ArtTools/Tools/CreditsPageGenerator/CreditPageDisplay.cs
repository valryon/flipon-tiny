using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditPageDisplay : MonoBehaviour
{
    public RectTransform creditPanel;
    public GameObject creditSectionPrefab;
    public CreditPage creditPage;

    private void Start()
    {
        PopulateCredits();
    }

    public void PopulateCredits()
    {
        if (creditPage == null || creditSectionPrefab == null || creditPanel == null)
        {
            Debug.LogError("Missing references in CreditPageDisplay.");
            return;
        }

        foreach (var entry in creditPage.credits)
        {
            GameObject creditSection = Instantiate(creditSectionPrefab, creditPanel);
            TMP_Text teamNameText = creditSection.transform.Find("TeamName").GetComponent<TMP_Text>();
            VerticalLayoutGroup namesLayoutGroup = creditSection.transform.Find("Names").GetComponent<VerticalLayoutGroup>();

            teamNameText.text = entry.teamName;
            teamNameText.fontSize = 30;
            foreach (string name in entry.names)
            {
                GameObject textGameObject = new GameObject("Name");
                textGameObject.AddComponent<RectTransform>().SetParent(namesLayoutGroup.gameObject.transform, false);
                TextMeshProUGUI nameText =textGameObject.AddComponent<TextMeshProUGUI>();
                nameText.text = name;
                nameText.fontSize = 25;
            }
        }
    }
}
