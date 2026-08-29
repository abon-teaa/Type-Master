using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public GameObject pausePanel;
    public GameObject pauseButton;
    public GameObject difficultyPanel;

    private bool isPaused = false;

    public void HidePauseButton()
    {
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void ShowPauseButton()
    {
        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (pauseButton != null)
            pauseButton.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenSettings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OpenSettingsPanel(pausePanel);
        }
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}