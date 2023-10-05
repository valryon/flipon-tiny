using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurrencyManager))]
public class CurrencyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CurrencyManager myScript = (CurrencyManager)target;

        if (GUILayout.Button("Add Currency"))
        {
            myScript.AddCurrency(100); 
        }

        if (GUILayout.Button("Remove Currency"))
        {
            myScript.RemoveCurrency(100); 
        }
    }
}
