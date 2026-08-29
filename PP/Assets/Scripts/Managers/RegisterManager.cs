using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField fullNameField, usernameField, emailField, ageField, passwordField, confirmPasswordField;
    public Button backButton, createAccountButton;
    public TMP_Text usernameStatusText, errorText;
    public GameObject loadingSpinner;

    void Start()
    {
        if (errorText) errorText.text = "";
        if (usernameStatusText) usernameStatusText.text = "";
        if (loadingSpinner) loadingSpinner.SetActive(false);
        if (ageField)
        {
            ageField.contentType = TMP_InputField.ContentType.IntegerNumber;
            ageField.ForceLabelUpdate();
        }
    }

    public void OnBackButtonPressed()
    {
        SceneLoader.LoadScene("HomeScene");
    }
    public void OnCreateAccountButtonPressed()
    {
        var nameInput = fullNameField ? fullNameField.text.Trim() : "";
        var userInput = usernameField ? usernameField.text.Trim() : "";
        var mailInput = emailField ? emailField.text.Trim().ToLower() : "";
        var pass = passwordField ? passwordField.text : "";
        var confPass = confirmPasswordField ? confirmPasswordField.text : "";
        if (!ValidationManager.IsNotEmpty(nameInput))
        {
            ShowError("Full name is required.");
            return;
        }
        if (!ValidationManager.IsValidUsername(userInput))
        {
            ShowError("Username must be 3-20 letters, numbers, or underscores.");
            return;
        }
        if (!ValidationManager.IsValidEmail(mailInput))
        {
            ShowError("Please enter a valid email address.");
            return;
        }
        int userAge = 0;
        bool hasValidAgeField = ageField && !string.IsNullOrEmpty(ageField.text);
        if (!hasValidAgeField || !int.TryParse(ageField.text, out userAge) || !ValidationManager.IsValidAge(ageField.text, out userAge))
        {
            ShowError("Please enter a valid age (13-120).");
            return;
        }
        if (!ValidationManager.IsStrongPassword(pass))
        {
            ShowError("Password needs 8+ characters, upper & lower case, and a number.");
            return;
        }
        if (!ValidationManager.DoPasswordsMatch(pass, confPass))
        {
            ShowError("Passwords do not match.");
            return;
        }
        SetLoading(true);
        if (!SupabaseManager.Instance)
        {
            ShowError("Database connection missing.");
            SetLoading(false);
            return;
        }
        SupabaseManager.Instance.CheckUsernameAvailable(userInput, (isOk, isAvailable) =>
        {
            if (!isOk)
            {
                ShowError("Couldn't check username availability. Try again.");
                return;
            }
            if (!isAvailable)
            {
                ShowError("Username is already taken. Please choose another.");
                return;
            }
            SupabaseManager.Instance.SignUp(mailInput, pass, (isSuccess, err, authData) =>
            {
                if (!isSuccess)
                {
                    ShowError(err);
                    return;
                }
                CompleteRegistration(authData.user.id, nameInput, userInput, mailInput, userAge, authData);
            });
        });
    }
    void CompleteRegistration(string uid, string fName, string uName, string mail, int ageVal, AuthResponse authData)
    {
        var newProfile = new NewUserProfile
        {
            id = uid,
            full_name = fName,
            username = uName,
            email = mail,
            age = ageVal,
            photo_url = ""
        };
        SupabaseManager.Instance.CreateUserProfile(newProfile, (saved, saveErr) =>
        {
            SetLoading(false);
            if (!saved)
            {
                ShowError("Account created, but saving profile failed: " + saveErr);
                return;
            }
            if (GameManager.Instance)
            {
                if (authData != null)
                {
                    GameManager.Instance.AccessToken = authData.access_token ?? "";
                    GameManager.Instance.RefreshToken = authData.refresh_token ?? "";
                }
                GameManager.Instance.CurrentUser = new UserModel
                {
                    id = uid,
                    full_name = fName,
                    username = uName,
                    email = mail,
                    age = ageVal,
                    photo_url = ""
                };
            }
            SceneLoader.LoadScene("HomeScene");
        });
    }
    void ShowError(string msg)
    {
        if (errorText) errorText.text = msg;
        SetLoading(false);
    }
    void SetLoading(bool loadingState)
    {
        if (loadingSpinner) loadingSpinner.SetActive(loadingState);
    }
}