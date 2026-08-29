using UnityEngine;
using TMPro;
public class LoginManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameOrEmailField;
    public TMP_InputField passwordField;
    [Header("UI Feedback")]
    public TMP_Text errorText;       
    public GameObject loadingSpinner; 
    private void Start()
    {
        if (errorText != null) errorText.text = "";
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }
    public void OnLoginButtonPressed()
    {
        string usernameOrEmail = usernameOrEmailField.text.Trim();
        string password = passwordField.text;
        if (!ValidationManager.IsNotEmpty(usernameOrEmail) || !ValidationManager.IsNotEmpty(password))
        {
            ShowError("Please fill in both fields.");
            return;
        }
        SetLoading(true);
        SupabaseManager.Instance.GetUserByUsernameOrEmail(usernameOrEmail, (found, user) =>
        {
            if (!found || user == null)
            {
                SetLoading(false);
                ShowError("Username or email not found.");
                return;
            }
            SupabaseManager.Instance.SignIn(user.email, password, (success, error, authResponse) =>
            {
                SetLoading(false);
                if (!success)
                {
                    ShowError("Incorrect password. Please try again.");
                    return;
                }
                GameManager.Instance.AccessToken = authResponse.access_token;
                GameManager.Instance.RefreshToken = authResponse.refresh_token;
                GameManager.Instance.CurrentUser = user;
                SceneLoader.LoadScene("ProfileScene");
            });
        });
    }
    public void OnForgotPasswordButtonPressed()
    {
        SceneLoader.LoadScene("ResetPasswordScene");
    }
    public void OnBackButtonPressed()
    {
        SceneLoader.LoadScene("HomeScene");
    }
    private void ShowError(string message)
    {
        if (errorText != null) errorText.text = message;
    }
    private void SetLoading(bool isLoading)
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(isLoading);
    }
}
