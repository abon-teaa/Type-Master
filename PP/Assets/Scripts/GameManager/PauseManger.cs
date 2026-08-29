using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public GameObject pausePanel, pauseButton, difficultyPanel;
    bool isPaused = false;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        if (pauseButton) pauseButton.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
    }
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
    }
    public void HidePauseButton()
    {
        if (pauseButton) pauseButton.SetActive(false);
    }
    public void ShowPauseButton()
    {
        if (pauseButton) pauseButton.SetActive(true);
    }
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(true);
        if (pauseButton) pauseButton.SetActive(false);
    }
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
        if (pauseButton) pauseButton.SetActive(true);
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ExitGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenuScene");
    }
    public void OpenSettings()
    {
        if (SettingsManager.Instance) SettingsManager.Instance.OpenSettingsPanel(pausePanel);
    }
    public bool IsPaused() => isPaused;
}