using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurrencyManager))]
public class CurrencyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CurrencyManager myScript = (CurrencyManager)target;

        if (GUILayout.Button("Add 1000 Currency"))
        {
            myScript.AddCurrency(1000); 
        }

        if (GUILayout.Button("Remove 100 Currency"))
        {
            myScript.RemoveCurrency(100); 
        }
    }
}
