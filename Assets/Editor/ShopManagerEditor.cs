using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShopManager))]
public class ShopManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default properties

        ShopManager shopManager = (ShopManager)target;
        if (GUILayout.Button("Unsell All Items"))
        {
            UnsellAllItems(shopManager);
        }
    }

    private void UnsellAllItems(ShopManager manager)
    {
        foreach (var item in manager.shopDatabase.GetAllItems())
        {
            item.isPurchased = false;
            item.isEnabled = false;
        }

        EditorUtility.SetDirty(manager.shopDatabase); // Marks the shopDatabase as changed so the changes can be saved.
        AssetDatabase.SaveAssets(); 
        Debug.Log("All items set to not purchased and not enabled!");
    }
}
