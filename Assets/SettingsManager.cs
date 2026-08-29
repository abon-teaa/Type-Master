using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public GameObject settingsPanel;

    public Button themeButton;
    public Sprite lightModeButtonIcon;
    public Sprite darkModeButtonIcon;

    public Button soundButton;
    public Sprite soundOnButtonIcon;
    public Sprite soundOffButtonIcon;

    public Button returnButton;

    // Background GameObjects (allows individual scaling/positioning)
    public GameObject lightBackground;
    public GameObject darkBackground;

    public Color lightModeTextColor = Color.black;
    public Color darkModeTextColor = Color.white;

    private Image themeButtonImage;
    private Image soundButtonImage;
    private bool isDarkMode = false;
    private bool isSoundOn = true;
    private GameObject previousPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (themeButton != null)
        {
            themeButtonImage = themeButton.GetComponent<Image>();
            themeButton.onClick.AddListener(OnThemeButtonClicked);
        }

        if (soundButton != null)
        {
            soundButtonImage = soundButton.GetComponent<Image>();
            soundButton.onClick.AddListener(OnSoundButtonClicked);
        }

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnButtonClicked);
        }

        ApplyTheme();
        ApplySound();
    }

    public void OnThemeButtonClicked()
    {
        isDarkMode = !isDarkMode;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (themeButtonImage != null)
        {
            themeButtonImage.sprite = isDarkMode ? darkModeButtonIcon : lightModeButtonIcon;
        }

        // Toggle individual background objects
        if (lightBackground != null) lightBackground.SetActive(!isDarkMode);
        if (darkBackground != null) darkBackground.SetActive(isDarkMode);

        UpdateExistingBombs();
    }

    public Color GetCurrentTextColor()
    {
        return isDarkMode ? darkModeTextColor : lightModeTextColor;
    }

    private void UpdateExistingBombs()
    {
        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (GameObject bomb in bombs)
        {
            TextMeshPro textMesh = bomb.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.color = GetCurrentTextColor();
            }
        }
    }

    public void OnSoundButtonClicked()
    {
        isSoundOn = !isSoundOn;
        ApplySound();
    }

    private void ApplySound()
    {
        AudioListener.volume = isSoundOn ? 1f : 0f;

        if (soundButtonImage != null)
        {
            soundButtonImage.sprite = isSoundOn ? soundOnButtonIcon : soundOffButtonIcon;
        }
    }

    public void OnReturnButtonClicked()
    {
        CloseSettingsPanel();
    }

    public void OpenSettingsPanel(GameObject sourcePanel = null)
    {
        previousPanel = sourcePanel;

        if (previousPanel != null)
            previousPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
            previousPanel = null;
        }
    }
}