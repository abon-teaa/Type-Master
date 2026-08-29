using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class HeartManager : MonoBehaviour
{
    public static HeartManager Instance;
    public int maxHearts = 5, currentHearts;
    public GameObject heartPrefab, gameOverPanel;
    public Transform heartContainer;
    public Sprite fullHeart, emptyHeart;
    public TextMeshProUGUI finalScoreText, highScoreText;
    List<Image> spawnedHearts = new List<Image>();
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
        foreach (Transform child in heartContainer) Destroy(child.gameObject);
        spawnedHearts.Clear();
        for (var i = 0; i < maxHearts; i++)
        {
            var heartImage = Instantiate(heartPrefab, heartContainer).GetComponent<Image>();
            if (heartImage) spawnedHearts.Add(heartImage);
        }
    }
    public void LoseHeart()
    {
        if (currentHearts <= 0) return;
        currentHearts--;
        UpdateHeartsUI();
        if (currentHearts <= 0) GameOver();
    }
    void UpdateHeartsUI()
    {
        for (var i = 0; i < spawnedHearts.Count; i++)
        {
            spawnedHearts[i].sprite = i < currentHearts ? fullHeart : emptyHeart;
        }
    }
    void GameOver()
    {
        Time.timeScale = 0f;
        var currentScore = 0;
        var highScore = 0;
        var modeName = "Easy";
        if (ScoreManager.Instance)
        {
            currentScore = ScoreManager.Instance.GetScore();
            highScore = ScoreManager.Instance.GetHighScore();
            modeName = ScoreManager.Instance.GetDifficultyName();
        }
        if (finalScoreText) finalScoreText.text = "Score: " + currentScore;
        if (highScoreText) highScoreText.text = modeName + " mode highest score: " + highScore;
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (PauseManager.Instance) PauseManager.Instance.HidePauseButton();
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (PauseManager.Instance) PauseManager.Instance.ShowPauseButton();
        if (ScoreManager.Instance) ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
    public void SetMaxHearts(int newMaxHearts)
    {
        maxHearts = newMaxHearts;
        currentHearts = Mathf.Min(currentHearts, maxHearts);
        GenerateHearts();
        UpdateHeartsUI();
    }
}