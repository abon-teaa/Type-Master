using UnityEngine;
using TMPro;
public class UserText : MonoBehaviour
{
    public TMP_InputField userText;
    void Start()
    {
        if (userText) userText.onEndEdit.AddListener(delegate { ButtonClicked(); });
    }
    public void ButtonClicked()
    {
        if (!userText) return;
        var typedWord = userText.text.Trim();
        if (string.IsNullOrEmpty(typedWord))
        {
            ClearInput();
            return;
        }
        if (CharacterAnimator.Instance) CharacterAnimator.Instance.PlayAction();
        var bombs = GameObject.FindGameObjectsWithTag("Bomb");
        var matchFound = false;
        foreach (var bomb in bombs)
        {
            var tmp = bomb.GetComponentInChildren<TextMeshPro>();
            if (tmp && tmp.text.Equals(typedWord, System.StringComparison.OrdinalIgnoreCase))
            {
                if (CharacterAnimator.Instance) CharacterAnimator.Instance.PlayJoy();
                if (SoundManager.Instance) SoundManager.Instance.PlayCorrectSound();
                if (ScoreManager.Instance)
                {
                    ScoreManager.Instance.AddScore();
                    if (ScoreManager.Instance.scorePopupPrefab)
                    {
                        var screenPos = Camera.main.WorldToScreenPoint(bomb.transform.position);
                        ScoreManager.Instance.ShowScorePopup(screenPos);
                    }
                }
                bomb.SetActive(false);
                Destroy(bomb);
                matchFound = true;
                break;
            }
        }
        if (!matchFound)
        {
            if (ScoreManager.Instance) ScoreManager.Instance.ResetCombo();
            if (SoundManager.Instance) SoundManager.Instance.PlayWrongSound();
        }
        ClearInput();
    }
    void ClearInput()
    {
        userText.text = "";
        userText.ActivateInputField();
    }
}