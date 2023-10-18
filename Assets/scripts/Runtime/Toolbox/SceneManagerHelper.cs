using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneManagerHelper : MonoBehaviour
{
    public static SceneManagerHelper Instance { get; private set; }

    private List<GameObject> disabledGameObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void DisableAllRootGameObjectsInScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            if (obj.activeSelf)
            {
                disabledGameObjects.Add(obj);
                obj.SetActive(false);
            }
        }
    }

    public void EnablePreviouslyDisabledGameObjects()
    {
        foreach (GameObject obj in disabledGameObjects)
        {
            obj.SetActive(true);
        }
        disabledGameObjects.Clear();
    }

    public void LoadSceneAdditively(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}
