using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void Play()
    {
        PlayerPrefs.SetInt("OpenSettings", 0);
        SceneManager.LoadScene("GameScene");
    }

    public void User()
    {
        SceneManager.LoadScene("ProfileScene");
    }

    public void OpenSettings()
    {
        PlayerPrefs.SetInt("OpenSettings", 1);
        SceneManager.LoadScene("GameScene");
    }
}