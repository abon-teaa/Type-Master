using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HeartManager : MonoBehaviour
{
    public static HeartManager Instance;
    public int maxHearts = 5;
    private int currentHearts;
    public GameObject heartPrefab;
    public Transform heartContainer;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    private List<Image> spawnedHearts = new List<Image>();
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHearts = maxHearts;
        GenerateHearts();
        UpdateHeartsUI();
    }

    void GenerateHearts()
    {
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedHearts.Clear();

        for (int i = 0; i < maxHearts; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            Image heartImage = newHeart.GetComponent<Image>();

            if (heartImage != null)
            {
                spawnedHearts.Add(heartImage);
            }
        }
    }

    public void LoseHeart()
    {
        if (currentHearts <= 0)
        {
            return;
        }
        currentHearts--;
        UpdateHeartsUI();

        if (currentHearts <= 0)
        {
            GameOver();
        }
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < spawnedHearts.Count; i++)
        {
            if (i < currentHearts)
            {
                spawnedHearts[i].sprite = fullHeart;
            }
            else
            {
                spawnedHearts[i].sprite = emptyHeart;
            }
        }
    }

    void GameOver()
    {
        Time.timeScale = 0f;

        int currentScore = 0;
        int highScore = 0;
        string modeName = "Easy"; 
        if (ScoreManager.Instance != null)
        {
            currentScore = ScoreManager.Instance.GetScore();
            highScore = ScoreManager.Instance.GetHighScore();
            modeName = ScoreManager.Instance.GetDifficultyName();
        }

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + currentScore;

        if (highScoreText != null)
            highScoreText.text = modeName + " mode highest score: " + highScore;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HidePauseButton();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowPauseButton();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetMaxHearts(int newMaxHearts)
    {
        maxHearts = newMaxHearts;
        currentHearts = Mathf.Min(currentHearts, maxHearts);
        GenerateHearts();
        UpdateHeartsUI();
    }
}