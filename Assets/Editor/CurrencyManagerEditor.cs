using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurrencyManager))]
public class CurrencyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CurrencyManager myScript = (CurrencyManager)target;

        if (GUILayout.Button("Add 10000 Currency"))
        {
            myScript.AddCurrency(10000);
            Debug.Log("Now you have " + myScript.GetCurrentCurrency() + " currency.");
        }

        if (GUILayout.Button("Add 1000 Currency"))
        {
            myScript.AddCurrency(1000);
            Debug.Log("Now you have " + myScript.GetCurrentCurrency() + " currency.");
        }

        if (GUILayout.Button("Remove 100 Currency"))
        {
            myScript.RemoveCurrency(100);
            Debug.Log("Now you have " + myScript.GetCurrentCurrency() + " currency.");
        }

        if (GUILayout.Button("Set Currency to 0"))
        {
            int curr = myScript.GetCurrentCurrency();
            myScript.RemoveCurrency(curr);
            Debug.Log("Now you have " + myScript.GetCurrentCurrency() + " currency.");
        }
    }
}
