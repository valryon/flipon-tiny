using UnityEngine;

[CreateAssetMenu(fileName = "NewCreditPage", menuName = "Game/Credit Page")]
public class CreditPage : ScriptableObject
{
    [System.Serializable]
    public class CreditEntry
    {
        public string teamName;
        public string[] names;
    }

    public CreditEntry[] credits;
}
