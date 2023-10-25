using System.IO;
using UnityEditor;
using UnityEngine;

public class ClearDataWindow : EditorWindow
{
	[MenuItem("Data/Clear All Data")]
	public static void ShowWindow()
	{
		string savePath = Path.Combine(Application.persistentDataPath, "playerData.dat");
		if (File.Exists(savePath))
		{
			File.Delete(savePath);
		}
    savePath = Path.Combine(Application.persistentDataPath, "tutorialData.dat");
    if (File.Exists(savePath))
    {
      File.Delete(savePath);
    }
  }
}