using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreditPageDisplay))]
public class CreditPageDisplayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CreditPageDisplay creditPageDisplay = (CreditPageDisplay)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Credits"))
        {
            creditPageDisplay.PopulateCredits();
        }
    }
}
