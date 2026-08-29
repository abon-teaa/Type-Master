using UnityEngine;
using TMPro;
public class ResetPasswordManager : MonoBehaviour
{
    [Header("Panels (only one active at a time)")]
    public GameObject panelEnterEmail;
    public GameObject panelEnterOtp;
    public GameObject panelNewPassword;
    public GameObject panelSuccess;
    [Header("Page 1: Enter Email / Username")]
    public TMP_InputField usernameOrEmailField;
    [Header("Page 2: Enter OTP")]
    public TMP_InputField otpField;
    [Header("Page 3: New Password")]
    public TMP_InputField newPasswordField;
    public TMP_InputField confirmPasswordField;
    [Header("Feedback")]
    public TMP_Text errorText;
    public GameObject loadingSpinner;
    private string targetEmail;
    private void Start()
    {
        ShowPanel(panelEnterEmail);
        if (errorText != null) errorText.text = "";
    }
    public void OnSendOtpButtonPressed()
    {
        string usernameOrEmail = usernameOrEmailField.text.Trim();
        if (!ValidationManager.IsNotEmpty(usernameOrEmail))
        {
            ShowError("Please enter your username or email.");
            return;
        }
        SetLoading(true);
        SupabaseManager.Instance.GetUserByUsernameOrEmail(usernameOrEmail, (found, user) =>
        {
            if (!found || user == null)
            {
                SetLoading(false);
                ShowError("No account found with that username or email.");
                return;
            }
            targetEmail = user.email;
            OTPManager.Instance.SendOtp(targetEmail, (success, message) =>
            {
                SetLoading(false);
                if (!success)
                {
                    ShowError(message);
                    return;
                }
                ShowPanel(panelEnterOtp);
            });
        });
    }
    public void OnVerifyOtpButtonPressed()
    {
        string otp = otpField.text.Trim();
        if (!ValidationManager.IsValidOtpFormat(otp))
        {
            ShowError("Enter the 6-digit code sent to your email.");
            return;
        }
        SetLoading(true);
        OTPManager.Instance.VerifyOtp(targetEmail, otp, (success, message) =>
        {
            SetLoading(false);
            if (!success)
            {
                ShowError(message);
                return;
            }
            ShowPanel(panelNewPassword);
        });
    }
    public void OnSavePasswordButtonPressed()
    {
        string newPassword = newPasswordField.text;
        string confirmPassword = confirmPasswordField.text;
        if (!ValidationManager.IsStrongPassword(newPassword))
        {
            ShowError("Password needs 8+ characters, upper & lower case, and a number.");
            return;
        }
        if (!ValidationManager.DoPasswordsMatch(newPassword, confirmPassword))
        {
            ShowError("Passwords do not match.");
            return;
        }
        SetLoading(true);
        SupabaseManager.Instance.GetUserByUsernameOrEmail(targetEmail, (found, user) =>
        {
            if (!found)
            {
                SetLoading(false);
                ShowError("Something went wrong. Please try again.");
                return;
            }
            SupabaseManager.Instance.UpdatePassword(GameManager.Instance.AccessToken, newPassword, (success, error) =>
            {
                SetLoading(false);
                if (!success)
                {
                    ShowError(error);
                    return;
                }
                ShowPanel(panelSuccess);
                Invoke(nameof(GoToLogin), 2f); 
            });
        });
    }
    private void GoToLogin()
    {
        SceneLoader.LoadScene("LoginScene");
    }
    private void ShowPanel(GameObject panelToShow)
    {
        panelEnterEmail.SetActive(panelToShow == panelEnterEmail);
        panelEnterOtp.SetActive(panelToShow == panelEnterOtp);
        panelNewPassword.SetActive(panelToShow == panelNewPassword);
        panelSuccess.SetActive(panelToShow == panelSuccess);
        if (errorText != null) errorText.text = "";
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
