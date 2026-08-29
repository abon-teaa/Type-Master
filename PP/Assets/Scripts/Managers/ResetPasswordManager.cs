using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ResetPasswordManager : MonoBehaviour
{
    public GameObject panelCheckUser, panelNewPassword, panelSuccess, loadingSpinner;
    public TMP_InputField usernameField, emailField, newPasswordField, confirmPasswordField;
    public Button verifyButton;
    public TMP_Text errorText;
    string targetUserId;
    void Start()
    {
        if (panelCheckUser) panelCheckUser.SetActive(true);
        if (panelNewPassword) panelNewPassword.SetActive(false);
        if (panelSuccess) panelSuccess.SetActive(false);
        if (errorText) errorText.text = "";
    }
    public void OnVerifyButtonPressed()
    {
        var uName = usernameField ? usernameField.text.Trim() : "";
        var mail = emailField ? emailField.text.Trim() : "";
        if (!ValidationManager.IsNotEmpty(uName) || !ValidationManager.IsNotEmpty(mail))
        {
            ShowError("Please enter both your username and email.");
            return;
        }
        SetLoading(true);
        SupabaseManager.Instance.GetUserByUsernameOrEmail(mail, (isFound, usr) =>
        {
            SetLoading(false);
            if (!isFound || usr == null || string.Compare(usr.username, uName, System.StringComparison.OrdinalIgnoreCase) != 0)
            {
                ShowError("Username and email do not match any existing account.");
                return;
            }
            targetUserId = usr.id;
            if (errorText) errorText.text = "";
            if (panelNewPassword) panelNewPassword.SetActive(true);
            if (newPasswordField)
            {
                newPasswordField.interactable = true;
                newPasswordField.ActivateInputField();
            }
            if (confirmPasswordField) confirmPasswordField.interactable = true;
            if (usernameField) usernameField.interactable = false;
            if (emailField) emailField.interactable = false;
            if (verifyButton) verifyButton.interactable = false;
        });
    }
    public void OnSavePasswordButtonPressed()
    {
        if (string.IsNullOrEmpty(targetUserId))
        {
            ShowError("Please verify your account details first.");
            return;
        }
        var mail = emailField ? emailField.text.Trim() : "";
        var newPass = newPasswordField ? newPasswordField.text : "";
        var confPass = confirmPasswordField ? confirmPasswordField.text : "";
        if (!ValidationManager.IsStrongPassword(newPass))
        {
            ShowError("Password needs 8+ characters, upper & lower case, and a number.");
            return;
        }
        if (!ValidationManager.DoPasswordsMatch(newPass, confPass))
        {
            ShowError("Passwords do not match.");
            return;
        }
        SetLoading(true);
        SupabaseManager.Instance.SignIn(mail, newPass, (logged, loginErr, res) =>
        {
            if (logged)
            {
                SetLoading(false);
                ShowError("New password cannot be the same as your current password.");
                return;
            }
            SupabaseManager.Instance.UpdatePassword(targetUserId, newPass, (updated, updateErr) =>
            {
                SetLoading(false);
                if (!updated)
                {
                    ShowError(updateErr);
                    return;
                }
                if (errorText) errorText.text = "";
                if (newPasswordField) newPasswordField.text = "";
                if (confirmPasswordField) confirmPasswordField.text = "";
                if (panelNewPassword) panelNewPassword.SetActive(true);
                if (panelSuccess) panelSuccess.SetActive(true);
            });
        });
    }
    public void OnBackButtonPressed()
    {
        var prev = PlayerPrefs.GetString("PreviousScene", "LoginScene");
        SceneLoader.LoadScene(prev);
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