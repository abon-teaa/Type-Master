using UnityEngine;
using TMPro;

public class UserText : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_InputField userText;

    void Start()
    {
        if (userText != null)
            userText.onEndEdit.AddListener(delegate { ButtonClicked(); });
    }

    public void ButtonClicked()
    {
        if (userText == null) return;

        string typedWord = userText.text.Trim();
        if (string.IsNullOrEmpty(typedWord))
        {
            ClearInput();
            return;
        }

        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        bool matchFound = false;

        foreach (GameObject bomb in bombs)
        {
            TextMeshPro tmp = bomb.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                if (tmp.text.Equals(typedWord, System.StringComparison.OrdinalIgnoreCase))
                {

                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlayCorrectSound();

                    if (ScoreManager.Instance != null)
                        ScoreManager.Instance.AddScore();

                    if (ScoreManager.Instance != null && ScoreManager.Instance.scorePopupPrefab != null)
                    {
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(bomb.transform.position);
                        ScoreManager.Instance.ShowScorePopup(screenPos);
                    }

                    bomb.SetActive(false);
                    Destroy(bomb);
                    matchFound = true;
                    break;
                }
            }
        }

        if (!matchFound)
        {

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayWrongSound();

            Debug.Log("No bomb found with the word: " + typedWord);
        }

        ClearInput();
    }

    private void ClearInput()
    {
        userText.text = "";
        userText.ActivateInputField();
    }
}