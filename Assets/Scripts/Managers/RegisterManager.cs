using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RegisterManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField fullNameField;
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField otpField;
    public TMP_InputField ageField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmPasswordField;
    [Header("Buttons")]
    public Button checkUsernameButton;
    public Button sendOtpButton;
    public Button verifyOtpButton;
    public Button createAccountButton;
    public Button choosePhotoButton;
    [Header("Profile Photo")]
    public Image profilePhotoPreview; 
    [Header("Status / Feedback Text")]
    public TMP_Text usernameStatusText;
    public TMP_Text otpStatusText;
    public TMP_Text errorText;
    public GameObject loadingSpinner;
    private bool isUsernameAvailable = false;
    private bool isOtpVerified = false;
    private byte[] selectedPhotoBytes = null;
    private string selectedPhotoFileName = null;
    private void Start()
    {
        if (errorText != null) errorText.text = "";
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }
    public void OnCheckUsernameButtonPressed()
    {
        string username = usernameField.text.Trim();
        if (!ValidationManager.IsValidUsername(username))
        {
            usernameStatusText.text = "Username must be 3-20 letters, numbers, or underscores.";
            isUsernameAvailable = false;
            return;
        }
        SupabaseManager.Instance.CheckUsernameAvailable(username, (requestOk, available) =>
        {
            if (!requestOk)
            {
                usernameStatusText.text = "Couldn't check username. Try again.";
                return;
            }
            isUsernameAvailable = available;
            usernameStatusText.text = available ? "Username is available!" : "Username is already taken.";
        });
    }
    public void OnChoosePhotoButtonPressed()
    {
        string filePath = NativeFilePickerBridge.OpenImageFile();
        if (string.IsNullOrEmpty(filePath)) return; 
        selectedPhotoBytes = File.ReadAllBytes(filePath);
        selectedPhotoFileName = Path.GetFileName(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(selectedPhotoBytes);
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
        if (profilePhotoPreview != null) profilePhotoPreview.sprite = sprite;
    }
    public void OnSendOtpButtonPressed()
    {
        string email = emailField.text.Trim();
        if (!ValidationManager.IsValidEmail(email))
        {
            ShowError("Please enter a valid email address.");
            return;
        }
        SetLoading(true);
        OTPManager.Instance.SendOtp(email, (success, message) =>
        {
            SetLoading(false);
            otpStatusText.text = success ? "OTP sent! Check your email." : message;
        });
    }
    public void OnVerifyOtpButtonPressed()
    {
        string email = emailField.text.Trim();
        string otp = otpField.text.Trim();
        if (!ValidationManager.IsValidOtpFormat(otp))
        {
            otpStatusText.text = "Enter the 6-digit code sent to your email.";
            return;
        }
        SetLoading(true);
        OTPManager.Instance.VerifyOtp(email, otp, (success, message) =>
        {
            SetLoading(false);
            isOtpVerified = success;
            otpStatusText.text = success ? "Email verified!" : message;
        });
    }
    public void OnCreateAccountButtonPressed()
    {
        string fullName = fullNameField.text.Trim();
        string username = usernameField.text.Trim();
        string email = emailField.text.Trim();
        string password = passwordField.text;
        string confirmPassword = confirmPasswordField.text;
        if (!ValidationManager.IsNotEmpty(fullName))
        {
            ShowError("Full name is required.");
            return;
        }
        if (!ValidationManager.IsValidUsername(username) || !isUsernameAvailable)
        {
            ShowError("Please choose and confirm an available username.");
            return;
        }
        if (!ValidationManager.IsValidEmail(email) || !isOtpVerified)
        {
            ShowError("Please verify your email with the OTP first.");
            return;
        }
        if (!ValidationManager.IsValidAge(ageField.text, out int age))
        {
            ShowError("Please enter a valid age (13-120).");
            return;
        }
        if (!ValidationManager.IsStrongPassword(password))
        {
            ShowError("Password needs 8+ characters, upper & lower case, and a number.");
            return;
        }
        if (!ValidationManager.DoPasswordsMatch(password, confirmPassword))
        {
            ShowError("Passwords do not match.");
            return;
        }
        SetLoading(true);
        SupabaseManager.Instance.SignUp(email, password, (success, error, authResponse) =>
        {
            if (!success)
            {
                SetLoading(false);
                ShowError(error);
                return;
            }
            string newUserId = authResponse.user.id;
            if (selectedPhotoBytes != null)
            {
                string storageFileName = $"{newUserId}_{selectedPhotoFileName}";
                SupabaseManager.Instance.UploadProfilePicture(storageFileName, selectedPhotoBytes, (uploadSuccess, photoUrlOrError) =>
                {
                    string photoUrl = uploadSuccess ? photoUrlOrError : "";
                    FinishRegistration(newUserId, fullName, username, email, age, photoUrl, authResponse);
                });
            }
            else
            {
                FinishRegistration(newUserId, fullName, username, email, age, "", authResponse);
            }
        });
    }
    private void FinishRegistration(string userId, string fullName, string username, string email,
        int age, string photoUrl, AuthResponse authResponse)
    {
        NewUserProfile profile = new NewUserProfile
        {
            id = userId,
            full_name = fullName,
            username = username,
            email = email,
            age = age,
            photo_url = photoUrl,
            high_score = 0
        };
        SupabaseManager.Instance.CreateUserProfile(profile, (success, error) =>
        {
            SetLoading(false);
            if (!success)
            {
                ShowError("Account created, but saving profile failed: " + error);
                return;
            }
            GameManager.Instance.AccessToken = authResponse.access_token;
            GameManager.Instance.RefreshToken = authResponse.refresh_token;
            GameManager.Instance.CurrentUser = new UserModel
            {
                id = userId,
                full_name = fullName,
                username = username,
                email = email,
                age = age,
                photo_url = photoUrl,
                high_score = 0
            };
            SceneLoader.LoadScene("ProfileScene");
        });
    }
    private void ShowError(string message)
    {
        if (errorText != null) errorText.text = message;
        SetLoading(false);
    }
    private void SetLoading(bool isLoading)
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(isLoading);
    }
}
public static class NativeFilePickerBridge
{
    public static string OpenImageFile()
    {
        Debug.LogWarning("NativeFilePickerBridge.OpenImageFile() is a placeholder. Plug in a real file picker plugin.");
        return null;
    }
}
