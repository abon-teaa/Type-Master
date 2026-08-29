using UnityEngine;
using UnityEngine.SceneManagement;
public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (Application.CanStreamedLevelBeLoaded(sceneName)) SceneManager.LoadScene(sceneName);
    }
}