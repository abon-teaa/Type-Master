using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombSpawnerScript : MonoBehaviour
{
    public static BombSpawnerScript Instance;

    public List<string> easyWords = new List<string>()
    {
        "DOG", "CAT", "RUN", "SUN", "MAP", "RED", "BIG", "HAT", "LEG", "EGG",
        "CUP", "BUG", "PEN", "FAN", "MAT", "NET", "JAM", "GUM", "ZAP", "FOG",
        "FLY", "SKY", "BOW", "COW", "PIG", "WIG", "MUG", "HUG", "KID", "LID",
        "NOD", "BUD", "MAD", "SAD", "HIT", "BIT", "LIP", "RIP", "ROB", "HOT",
        "WET", "DRY", "FUN", "RUN", "CUT", "HUG", "JOG", "LOG", "FOX", "BOX",
    };

    public List<string> mediumWords = new List<string>()
    {
        "APPLE", "BANJO", "CANDY", "DANCE", "EAGLE", "FANCY", "GRAPE", "HEART", "ICING", "JOKER",
        "LUNAR", "MIGHT", "NIGHT", "OCEAN", "PEACH", "QUEEN", "RIVER", "STONE", "TIGER", "UNITY",
        "VIVID", "WINDY", "EXTRA", "YOUTH", "ZEBRA", "BLOOM", "CRISP", "DRIFT", "EAGER", "FLOOD",
        "GRAND", "HONOR", "IMAGE", "JOLLY", "KNIFE", "LEMON", "MERRY", "NOBLE", "ORBIT", "PRIZE",
        "QUIET", "RHYTH", "SILKY", "TRAIL", "ULTRA", "VALOR", "WALTZ", "XENON", "YEARN", "ZESTY",
    };

    public List<string> hardWords = new List<string>()
    {
        "ABYSSAL", "BANISH", "CALLOUS", "DECREPIT", "ENIGMA", "FALLACY", "GHASTLY", "HARBINGER", "IGNOMINY", "JADED",
        "KNAVELY", "LABYRINTH", "MALEVOLENT", "NEBULOUS", "OPULENT", "PARADOX", "QUAGMIRE", "RENEGADE", "SURREPTITIOUS", "TACITURN",
        "UBIQUITOUS", "VERBOSE", "WHIMSICAL", "XENOPHOBIA", "ZEALOT", "ACQUIESCE", "BENEVOLENT", "CACOPHONY", "DILAPIDATED", "EPHEMERAL",
        "FACETIOUS", "GREGARIOUS", "HACKNEYED", "IDIOSYNCRASY", "JUXTAPOSE", "KALEIDOSCOPE", "LUGUBRIOUS", "MISCELLANEOUS", "NOSTALGIA", "ONEROUS",
        "OBSTREPEROUS", "PECULIAR", "QUINTESSENTIAL", "RECALCITRANT", "SCRUPULOUS", "TANTAMOUNT", "UNPREDICTABLE", "VENERABLE", "WONDROUS", "ZIGGURAT"
    };

    private List<string> currentWordList = new List<string>();
    private List<string> shuffledWords = new List<string>();
    private int currentWordIndex = 0;

    [Header("Spawning Settings")]
    public GameObject Bomb;
    public float spawnInterval = 2f;

    [Header("Spawn Boundaries")]
    public float minX = -15f;
    public float maxX = 15f;

    private float timer;
    public bool gameStarted = false;

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
        if (!gameStarted) return;

        if (Bomb == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            float randomX = UnityEngine.Random.Range(minX, maxX);
            UnityEngine.Vector3 spawnPosition = new UnityEngine.Vector3(randomX, transform.position.y, 0);
            GameObject newBomb = Instantiate(Bomb, spawnPosition, UnityEngine.Quaternion.identity);
            BombBehaviour bombScript = newBomb.GetComponent<BombBehaviour>();
            if (bombScript != null)
            {
                string word = GetNextWord();
                bombScript.SetWord(word);
            }
            StartCoroutine(AutoDestroyAfterDelay(newBomb, 10f));
            timer = 0f;
        }
    }

    public GameObject difficultyPanel;

    public void SetDifficulty(int level)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetDifficulty(level);
        }

        if (level == 0)
        {
            currentWordList = new List<string>(easyWords);
            spawnInterval = 3.4f;
            if (HeartManager.Instance != null) HeartManager.Instance.SetMaxHearts(5);
        }
        else if (level == 1)
        {
            currentWordList = new List<string>(mediumWords);
            spawnInterval = 3f;
            if (HeartManager.Instance != null) HeartManager.Instance.SetMaxHearts(3);
        }
        else if (level == 2)
        {
            currentWordList = new List<string>(hardWords);
            spawnInterval = 2.8f;
            if (HeartManager.Instance != null) HeartManager.Instance.SetMaxHearts(1);
        }

        ShuffleWords();
        timer = 0f;
        gameStarted = true;
        Time.timeScale = 1f;

        if (PauseManager.Instance != null && PauseManager.Instance.pauseButton != null)
            PauseManager.Instance.pauseButton.SetActive(true);

        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
    }

    IEnumerator AutoDestroyAfterDelay(GameObject bomb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bomb != null)
        {
            bomb.SetActive(false);
            Destroy(bomb);
        }
    }

    void ShuffleWords()
    {
        shuffledWords = new List<string>(currentWordList);
        for (int i = shuffledWords.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string temp = shuffledWords[i];
            shuffledWords[i] = shuffledWords[j];
            shuffledWords[j] = temp;
        }
        currentWordIndex = 0;
    }

    string GetNextWord()
    {
        if (shuffledWords.Count == 0) 
        {
            return "BOMB";
        }

        if (currentWordIndex >= shuffledWords.Count)
        {
            ShuffleWords();
        }
        return shuffledWords[currentWordIndex++];
    }
}