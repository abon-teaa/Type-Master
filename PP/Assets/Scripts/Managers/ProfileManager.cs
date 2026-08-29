using System.Collections;
using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class ProfileManager : MonoBehaviour
{
    public Image profilePicture;
    public TMP_Text usernameText, fullNameText, emailText, ageText;
    public Button changePasswordButton, logoutButton, addPfpButton;
    void Start()
    {
        var gm = GameManager.Instance;
        if (!gm || !gm.IsLoggedIn())
        {
            SceneLoader.LoadScene("HomeScene");
            return;
        }
        DisplayUserData();
    }
    void DisplayUserData()
    {
        var gm = GameManager.Instance;
        var u = gm ? gm.CurrentUser : null;
        if (u == null) return;
        if (usernameText) usernameText.text = u.username ?? "";
        if (fullNameText) fullNameText.text = u.full_name ?? "";
        if (emailText) emailText.text = u.email ?? "";
        if (ageText) ageText.text = u.age.ToString();
        if (profilePicture && !string.IsNullOrEmpty(u.photo_url)) StartCoroutine(LoadProfilePicture(u.photo_url));
    }
    public void OnAddProfilePicturePressed()
    {
        var ext = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Profile Picture", "", ext, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0])) StartCoroutine(UploadAndSetProfilePicture(paths[0]));
    }
    IEnumerator UploadAndSetProfilePicture(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2);
        if (tex.LoadImage(bytes))
        {
            var imgSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            if (profilePicture) profilePicture.sprite = imgSprite;
        }
        var u = GameManager.Instance.CurrentUser;
        var fName = $"avatar_{u.id}_{System.DateTime.Now.Ticks}.png";
        if (SupabaseManager.Instance)
        {
            SupabaseManager.Instance.UploadProfileImage(u.id, fName, bytes, (ok, imgUrl) =>
            {
                if (ok) u.photo_url = imgUrl;
            });
        }
        yield return null;
    }
    IEnumerator LoadProfilePicture(string imgUrl)
    {
        using (var req = UnityWebRequestTexture.GetTexture(imgUrl))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var tex = DownloadHandlerTexture.GetContent(req);
                var imgSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                if (profilePicture) profilePicture.sprite = imgSprite;
            }
        }
    }
    public void OnForgotPasswordButtonPressed()
    {
        PlayerPrefs.SetString("PreviousScene", "ProfileScene");
        SceneLoader.LoadScene("ResetPassScene");
    }
    public void OnChangePasswordButtonPressed()
    {
        PlayerPrefs.SetString("PreviousScene", "ProfileScene");
        SceneLoader.LoadScene("ResetPassScene");
    }
    public void OnBackButtonPressed()
    {
        SceneLoader.LoadScene("MainMenuScene");
    }
    public void OnLogoutButtonPressed()
    {
        if (SupabaseManager.Instance)
        {
            SupabaseManager.Instance.SignOut((ok, err) =>
            {
                if (GameManager.Instance) GameManager.Instance.ClearSession();
                SceneLoader.LoadScene("HomeScene");
            });
        }
        else
        {
            if (GameManager.Instance) GameManager.Instance.ClearSession();
            SceneLoader.LoadScene("HomeScene");
        }
    }
}