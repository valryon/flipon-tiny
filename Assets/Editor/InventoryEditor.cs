using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(InventoryManager))]
public class InventoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // This will draw the default inspector properties

        InventoryManager inventoryManager = (InventoryManager)target;
        if (GUILayout.Button("Clear Player Inventory"))
        {
            ClearPlayerInventory(inventoryManager);
        }
    }

    private void ClearPlayerInventory(InventoryManager manager)
    {
        string path = Path.Combine(Application.persistentDataPath, InventoryManager.INVENTORY_FILENAME);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        manager.playerInventory.items.Clear(); // Clear the in-memory items list
        manager.PopulateInventoryUI();
        Debug.Log("Player inventory cleared!");
    }
}
