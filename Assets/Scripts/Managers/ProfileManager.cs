using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
public class ProfileManager : MonoBehaviour
{
    [Header("Display Fields")]
    public Image profilePicture;
    public TMP_Text usernameText;
    public TMP_Text fullNameText;
    public TMP_Text emailText;
    public TMP_Text ageText;
    public TMP_Text highScoreText;
    [Header("Buttons")]
    public Button changePasswordButton;
    public Button logoutButton;
    private void Start()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsLoggedIn())
        {
            SceneLoader.LoadScene("HomeScene");
            return;
        }
        DisplayUserData();
    }
    private void DisplayUserData()
    {
        UserModel user = GameManager.Instance.CurrentUser;
        usernameText.text = user.username;
        fullNameText.text = user.full_name;
        emailText.text = user.email;
        ageText.text = user.age.ToString();
        highScoreText.text = user.high_score.ToString();
        if (!string.IsNullOrEmpty(user.photo_url))
        {
            StartCoroutine(LoadProfilePicture(user.photo_url));
        }
    }
    private IEnumerator LoadProfilePicture(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                profilePicture.sprite = sprite;
            }
            else
            {
                Debug.LogWarning("Could not load profile picture: " + request.error);
            }
        }
    }
    public void OnChangePasswordButtonPressed()
    {
        SceneLoader.LoadScene("ResetPasswordScene");
    }
    public void OnLogoutButtonPressed()
    {
        SupabaseManager.Instance.SignOut((success, error) =>
        {
            GameManager.Instance.ClearSession();
            SceneLoader.LoadScene("HomeScene");
        });
    }
}
