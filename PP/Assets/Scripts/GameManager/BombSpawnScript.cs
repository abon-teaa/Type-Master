using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombSpawnerScript : MonoBehaviour
{
    public static BombSpawnerScript Instance;
    public GameObject Bomb, difficultyPanel;
    public float spawnInterval = 2f, minX = -15f, maxX = 15f;
    public bool gameStarted = false;

    List<string> currentWordList = new List<string>(), shuffledWords = new List<string>();
    int currentWordIndex = 0;
    float timer;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        timer = 0f;
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!gameStarted || !Bomb) return;
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            var randomX = Random.Range(minX, maxX);
            var spawnPosition = new Vector3(randomX, transform.position.y, 0);
            var newBomb = Instantiate(Bomb, spawnPosition, Quaternion.identity);
            var bombScript = newBomb.GetComponent<BombBehaviour>();
            if (bombScript) bombScript.SetWord(GetNextWord());
            StartCoroutine(AutoDestroyAfterDelay(newBomb, 10f));
            timer = 0f;
        }
    }

    public void SetDifficulty(int level)
    {
        if (ScoreManager.Instance) ScoreManager.Instance.SetDifficulty(level);

        int wordLength = 4;
        if (level == 0)
        {
            wordLength = 4;
            spawnInterval = 3.4f;
            if (HeartManager.Instance) HeartManager.Instance.SetMaxHearts(5);
        }
        else if (level == 1)
        {
            wordLength = 6;
            spawnInterval = 3f;
            if (HeartManager.Instance) HeartManager.Instance.SetMaxHearts(3);
        }
        else if (level == 2)
        {
            wordLength = 8;
            spawnInterval = 2.8f;
            if (HeartManager.Instance) HeartManager.Instance.SetMaxHearts(1);
        }

        StartCoroutine(FetchWordsRoutine(wordLength, 50));
    }

    IEnumerator FetchWordsRoutine(int wordLength, int amount)
    {
        string url = $"https://random-word-api.herokuapp.com/word?number={amount}&length={wordLength}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                currentWordList = ParseJsonResponse(request.downloadHandler.text);
            }
            else
            {
                currentWordList = GetFallbackWords(wordLength);
            }
        }

        ShuffleWords();
        timer = 0f;
        gameStarted = true;
        Time.timeScale = 1f;
        if (PauseManager.Instance && PauseManager.Instance.pauseButton) PauseManager.Instance.pauseButton.SetActive(true);
        if (difficultyPanel) difficultyPanel.SetActive(false);
    }

    List<string> ParseJsonResponse(string json)
    {
        List<string> words = new List<string>();
        json = json.Trim('[', ']', ' ', '\n', '\r');
        string[] items = json.Split(',');

        foreach (string item in items)
        {
            string cleanWord = item.Trim('"', ' ').ToUpper();
            if (!string.IsNullOrEmpty(cleanWord)) words.Add(cleanWord);
        }
        return words.Count > 0 ? words : GetFallbackWords(4);
    }

    List<string> GetFallbackWords(int length)
    {
        if (length == 4) return new List<string> { "BOMB", "DROP", "FIRE", "WORD", "GAME" };
        if (length == 6) return new List<string> { "ACTION", "DANGER", "PLAYER", "SHIELD", "SYSTEM" };
        return new List<string> { "EXPLODE", "TERMINAL", "SECURITY", "HARDWARE", "SOFTWARE" };
    }

    IEnumerator AutoDestroyAfterDelay(GameObject bomb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bomb)
        {
            bomb.SetActive(false);
            Destroy(bomb);
        }
    }

    void ShuffleWords()
    {
        shuffledWords = new List<string>(currentWordList);
        for (var i = shuffledWords.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            var temp = shuffledWords[i];
            shuffledWords[i] = shuffledWords[j];
            shuffledWords[j] = temp;
        }
        currentWordIndex = 0;
    }

    string GetNextWord()
    {
        if (shuffledWords.Count == 0) return "BOMB";
        if (currentWordIndex >= shuffledWords.Count) ShuffleWords();
        return shuffledWords[currentWordIndex++];
    }
}