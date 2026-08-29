using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    public GameObject settingsPanel, levelPanel, lightBackground, darkBackground;
    public Button themeButton, soundButton, returnButton;
    public Sprite lightModeButtonIcon, darkModeButtonIcon, soundOnButtonIcon, soundOffButtonIcon;
    public Color lightModeTextColor = Color.black, darkModeTextColor = Color.white;
    Image themeButtonImage, soundButtonImage;
    static bool isDarkMode = false, isSoundOn = true;
    GameObject previousPanel;
    bool openedFromMainMenu = false;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        isDarkMode = false;
        isSoundOn = true;
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        if (themeButton)
        {
            themeButtonImage = themeButton.GetComponent<Image>();
            themeButton.onClick.AddListener(OnThemeButtonClicked);
        }
        if (soundButton)
        {
            soundButtonImage = soundButton.GetComponent<Image>();
            soundButton.onClick.AddListener(OnSoundButtonClicked);
        }
        if (returnButton) returnButton.onClick.AddListener(OnReturnButtonClicked);
        ApplyTheme();
        ApplySound();
        if (PlayerPrefs.GetInt("OpenSettings", 0) == 1)
        {
            PlayerPrefs.SetInt("OpenSettings", 0);
            openedFromMainMenu = true;
            if (levelPanel) levelPanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(true);
        }
    }
    public void OnThemeButtonClicked()
    {
        isDarkMode = !isDarkMode;
        ApplyTheme();
    }
    void ApplyTheme()
    {
        if (themeButtonImage) themeButtonImage.sprite = isDarkMode ? darkModeButtonIcon : lightModeButtonIcon;
        if (lightBackground) lightBackground.SetActive(!isDarkMode);
        if (darkBackground) darkBackground.SetActive(isDarkMode);
        UpdateExistingBombs();
    }
    public Color GetCurrentTextColor()
    {
        return isDarkMode ? darkModeTextColor : lightModeTextColor;
    }
    void UpdateExistingBombs()
    {
        var bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (var bomb in bombs)
        {
            var textMesh = bomb.GetComponentInChildren<TextMeshPro>();
            if (textMesh) textMesh.color = GetCurrentTextColor();
        }
    }
    public void OnSoundButtonClicked()
    {
        isSoundOn = !isSoundOn;
        ApplySound();
    }
    void ApplySound()
    {
        AudioListener.volume = isSoundOn ? 1f : 0f;
        if (soundButtonImage) soundButtonImage.sprite = isSoundOn ? soundOnButtonIcon : soundOffButtonIcon;
    }
    public void OnReturnButtonClicked()
    {
        if (openedFromMainMenu)
        {
            openedFromMainMenu = false;
            SceneManager.LoadScene("MainMenuScene");
        }
        else CloseSettingsPanel();
    }
    public void OpenSettingsPanel(GameObject sourcePanel = null)
    {
        previousPanel = sourcePanel;
        if (previousPanel) previousPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }
    public void CloseSettingsPanel()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (previousPanel)
        {
            previousPanel.SetActive(true);
            previousPanel = null;
        }
    }
}