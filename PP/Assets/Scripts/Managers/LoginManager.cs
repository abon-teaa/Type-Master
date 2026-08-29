using UnityEngine;
using TMPro;
public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameOrEmailField, passwordField;
    public TMP_Text errorText;
    public GameObject loadingSpinner;
    void Start()
    {
        if (errorText) errorText.text = "";
        if (loadingSpinner) loadingSpinner.SetActive(false);
    }
    public void OnLoginButtonPressed()
    {
        var inputUser = usernameOrEmailField ? usernameOrEmailField.text.Trim() : "";
        var pass = passwordField ? passwordField.text : "";
        if (!ValidationManager.IsNotEmpty(inputUser) || !ValidationManager.IsNotEmpty(pass))
        {
            ShowError("Please fill in both fields.");
            return;
        }
        if (!SupabaseManager.Instance)
        {
            ShowError("System error: Database connection unavailable.");
            return;
        }
        if (errorText) errorText.text = "";
        SetLoading(true);
        SupabaseManager.Instance.GetUserByUsernameOrEmail(inputUser, (found, user) =>
        {
            if (!found || user == null)
            {
                SetLoading(false);
                ShowError("Username or email not found.");
                return;
            }
            if (string.IsNullOrEmpty(user.email))
            {
                SetLoading(false);
                ShowError("No email associated with this account.");
                return;
            }
            SupabaseManager.Instance.SignIn(user.email, pass, (ok, err, auth) =>
            {
                SetLoading(false);
                if (!ok)
                {
                    ShowError(!string.IsNullOrEmpty(err) ? err : "Incorrect password. Please try again.");
                    return;
                }
                if (GameManager.Instance)
                {
                    if (auth != null)
                    {
                        GameManager.Instance.AccessToken = auth.access_token ?? "";
                        GameManager.Instance.RefreshToken = auth.refresh_token ?? "";
                    }
                    GameManager.Instance.CurrentUser = user;
                }
                SceneLoader.LoadScene("MainMenuScene");
            });
        });
    }
    public void OnForgotPasswordButtonPressed()
    {
        PlayerPrefs.SetString("PreviousScene", "LoginScene");
        SceneLoader.LoadScene("ResetPassScene");
    }
    public void OnBackButtonPressed()
    {
        SceneLoader.LoadScene("HomeScene");
    }
    void ShowError(string msg)
    {
        if (errorText) errorText.text = msg;
        SetLoading(false);
    }
    void SetLoading(bool state)
    {
        if (loadingSpinner) loadingSpinner.SetActive(state);
    }
}