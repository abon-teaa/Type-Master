using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject scorePopupPrefab;
    public Canvas canvas;
    public int pointsPerWord = 1;
    private int currentScore = 0;
    private int currentDifficulty = 0; 

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        UpdateUI();
    }

    public void AddScore(int points = -1)
    {
        if (points == -1) points = pointsPerWord;
        currentScore += points;

        string key = GetHighScoreKey();
        int currentHighScore = PlayerPrefs.GetInt(key, 0);
        if (currentScore > currentHighScore)
        {
            PlayerPrefs.SetInt(key, currentScore);
            Debug.Log($"🎉 New high score for {GetDifficultyName()}: {currentScore}");
        }
        UpdateUI();
    }

    public void SetDifficulty(int difficulty)
    {
        currentDifficulty = difficulty;
        currentScore = 0;
        UpdateUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
    }

    public int GetScore() => currentScore;

    public int GetHighScore()
    {
        string key = GetHighScoreKey();
        return PlayerPrefs.GetInt(key, 0);
    }

    private string GetHighScoreKey()
    {
        return "HighScore_" + GetDifficultyName();
    }

    public string GetDifficultyName()
    {
        switch (currentDifficulty)
        {
            case 0: return "Easy";
            case 1: return "Medium";
            case 2: return "Hard";
            default: return "Easy";
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

        if (highScoreText != null)
        {
            int high = GetHighScore();
            highScoreText.text = "Best: " + high;
        }
    }

    public void ShowScorePopup(Vector3 screenPosition)
    {
        if (scorePopupPrefab == null)
        {
            Debug.LogWarning("Score popup prefab not assigned.");
            return;
        }

        Canvas targetCanvas = canvas != null ? canvas : FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            Debug.LogWarning("No Canvas found for popup.");
            return;
        }

        GameObject popup = Instantiate(scorePopupPrefab, targetCanvas.transform);
        RectTransform rect = popup.GetComponent<RectTransform>();
        if (rect != null)
            rect.position = screenPosition;
        else
            popup.transform.position = screenPosition;
    }
}