using System.Collections;
using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public TextMeshProUGUI scoreText, highScoreText, doubleText, tripleText, doubleMultiplierText, tripleMultiplierText, newHighScoreText;
    public GameObject scorePopupPrefab;
    public Canvas canvas;
    public int pointsPerWord = 1, currentScore = 0, currentDifficulty = 0;
    int comboCount = 0, thresh2x = 8, thresh3x = 16, currentBest = 0;
    bool highScoreBeaten = false;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        UpdateUI();
    }
    public void SetDifficulty(int difficulty)
    {
        currentDifficulty = difficulty;
        currentScore = 0;
        comboCount = 0;
        highScoreBeaten = false;
        currentBest = GetHighScore();
        if (difficulty == 0) { thresh2x = 8; thresh3x = 16; }
        else if (difficulty == 1) { thresh2x = 6; thresh3x = 12; }
        else if (difficulty == 2) { thresh2x = 4; thresh3x = 8; }
        UpdateUI();
    }
    public void AddScore(int points = -1)
    {
        comboCount++;
        int multiplier = 1;
        if (comboCount >= thresh3x) multiplier = 3;
        else if (comboCount >= thresh2x) multiplier = 2;
        if (points == -1) points = pointsPerWord;
        currentScore += points * multiplier;
        var key = GetHighScoreKey();
        if (currentScore > currentBest && currentBest > 0 && !highScoreBeaten)
        {
            highScoreBeaten = true;
            if (newHighScoreText) ShowCombo(newHighScoreText);
        }
        if (currentScore > PlayerPrefs.GetInt(key, 0)) PlayerPrefs.SetInt(key, currentScore);
        if (comboCount == thresh2x) ShowCombo(doubleText);
        else if (comboCount == thresh3x) ShowCombo(tripleText);
        UpdateUI();
    }
    public void ResetCombo()
    {
        comboCount = 0;
        StopAllCoroutines();
        if (doubleText) doubleText.gameObject.SetActive(false);
        if (tripleText) tripleText.gameObject.SetActive(false);
        UpdateUI();
    }
    public void ResetScore()
    {
        currentScore = 0;
        highScoreBeaten = false;
        ResetCombo();
    }
    public int GetScore() => currentScore;
    public int GetHighScore() => PlayerPrefs.GetInt(GetHighScoreKey(), 0);
    string GetHighScoreKey() => "HighScore_" + GetDifficultyName();
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
    void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + currentScore;
        if (doubleMultiplierText) doubleMultiplierText.gameObject.SetActive(comboCount >= thresh2x && comboCount < thresh3x);
        if (tripleMultiplierText) tripleMultiplierText.gameObject.SetActive(comboCount >= thresh3x);
    }
    public void ShowScorePopup(Vector3 screenPosition)
    {
        if (!scorePopupPrefab) return;
        var targetCanvas = canvas ? canvas : FindAnyObjectByType<Canvas>();
        if (!targetCanvas) return;
        var popup = Instantiate(scorePopupPrefab, targetCanvas.transform);
        var rect = popup.GetComponent<RectTransform>();
        if (rect) rect.position = screenPosition;
        else popup.transform.position = screenPosition;
    }
    void ShowCombo(TextMeshProUGUI targetText)
    {
        if (!targetText) return;
        StopAllCoroutines();
        StartCoroutine(DisplayCombo(targetText));
    }
    IEnumerator DisplayCombo(TextMeshProUGUI targetText)
    {
        if (doubleText && targetText != doubleText) doubleText.gameObject.SetActive(false);
        if (tripleText && targetText != tripleText) tripleText.gameObject.SetActive(false);
        if (newHighScoreText && targetText != newHighScoreText) newHighScoreText.gameObject.SetActive(false);
        targetText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.9f);
        targetText.gameObject.SetActive(false);
    }
}