using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class SupabaseManager : MonoBehaviour
{
    private static SupabaseManager _instance;
    public string supabaseUrl = "https://yvorywyknvczifggvylf.supabase.co";
    public string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inl2b3J5d3lrbnZjemlmZ2d2eWxmIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODYzODA0MDMsImV4cCI6MjEwMTk1MjQwM30.xbb7VBYZhYC8e2poylGssOQ8QLQoH5_xpnsWC2O9_28";
    public string supabaseServiceRoleKey = "";
    public static SupabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SupabaseManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SupabaseManager");
                    _instance = go.AddComponent<SupabaseManager>();
                }
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SanitizeCredentials();
    }
    private void SanitizeCredentials()
    {
        if (!string.IsNullOrEmpty(supabaseUrl)) supabaseUrl = supabaseUrl.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(supabaseAnonKey)) supabaseAnonKey = supabaseAnonKey.Trim();
        if (!string.IsNullOrEmpty(supabaseServiceRoleKey)) supabaseServiceRoleKey = supabaseServiceRoleKey.Trim();
    }
    public void SignUp(string email, string password, Action<bool, string, AuthResponse> onComplete)
    {
        StartCoroutine(SignUpRoutine(email, password, onComplete));
    }
    private IEnumerator SignUpRoutine(string email, string password, Action<bool, string, AuthResponse> onComplete)
    {
        string url = $"{supabaseUrl}/auth/v1/signup";
        SignUpRequest body = new SignUpRequest { email = email, password = password };
        using (UnityWebRequest request = BuildPostRequest(url, JsonUtility.ToJson(body), false))
        {
            yield return request.SendWebRequest();
            HandleAuthResponse(request, onComplete);
        }
    }
    public void SignIn(string email, string password, Action<bool, string, AuthResponse> onComplete)
    {
        StartCoroutine(SignInRoutine(email, password, onComplete));
    }
    private IEnumerator SignInRoutine(string email, string password, Action<bool, string, AuthResponse> onComplete)
    {
        string url = $"{supabaseUrl}/auth/v1/token?grant_type=password";
        SignInRequest body = new SignInRequest { email = email, password = password };
        using (UnityWebRequest request = BuildPostRequest(url, JsonUtility.ToJson(body), false))
        {
            yield return request.SendWebRequest();
            HandleAuthResponse(request, onComplete);
        }
    }
    public void SignOut(Action<bool, string> onComplete)
    {
        StartCoroutine(SignOutRoutine(onComplete));
    }
    private IEnumerator SignOutRoutine(Action<bool, string> onComplete)
    {
        string url = $"{supabaseUrl}/auth/v1/logout";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", supabaseAnonKey);
            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.AccessToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {GameManager.Instance.AccessToken}");
            }
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) onComplete?.Invoke(true, null);
            else onComplete?.Invoke(false, ExtractErrorMessage(request));
        }
    }
    public void UpdatePassword(string userId, string newPassword, Action<bool, string> onComplete)
    {
        StartCoroutine(UpdatePasswordRoutine(userId, newPassword, onComplete));
    }
    private IEnumerator UpdatePasswordRoutine(string userId, string newPassword, Action<bool, string> onComplete)
    {
        if (string.IsNullOrEmpty(userId))
        {
            onComplete?.Invoke(false, "User ID missing. Please verify account first.");
            yield break;
        }
        string authKey = !string.IsNullOrEmpty(supabaseServiceRoleKey) ? supabaseServiceRoleKey : supabaseAnonKey;
        string url = !string.IsNullOrEmpty(supabaseServiceRoleKey) ? $"{supabaseUrl}/auth/v1/admin/users/{userId}" : $"{supabaseUrl}/auth/v1/user";
        UpdatePasswordRequest body = new UpdatePasswordRequest { password = newPassword };
        byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", authKey);
            if (!string.IsNullOrEmpty(supabaseServiceRoleKey)) request.SetRequestHeader("Authorization", $"Bearer {supabaseServiceRoleKey}");
            else if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.AccessToken)) request.SetRequestHeader("Authorization", $"Bearer {GameManager.Instance.AccessToken}");
            else
            {
                onComplete?.Invoke(false, "User session token missing. Please log in first or add Service Role Key.");
                yield break;
            }
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) onComplete?.Invoke(true, null);
            else onComplete?.Invoke(false, ExtractErrorMessage(request));
        }
    }
    public void CheckUsernameAvailable(string username, Action<bool, bool> onComplete)
    {
        StartCoroutine(CheckUsernameRoutine(username, onComplete));
    }
    private IEnumerator CheckUsernameRoutine(string username, Action<bool, bool> onComplete)
    {
        string url = $"{supabaseUrl}/rest/v1/users?select=id&username=eq.{UnityWebRequest.EscapeURL(username)}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                ExtractErrorMessage(request);
                onComplete?.Invoke(false, false);
                yield break;
            }
            bool isAvailable = request.downloadHandler.text.Trim() == "[]";
            onComplete?.Invoke(true, isAvailable);
        }
    }
    public void CreateUserProfile(NewUserProfile profile, Action<bool, string> onComplete)
    {
        StartCoroutine(CreateUserProfileRoutine(profile, onComplete));
    }
    private IEnumerator CreateUserProfileRoutine(NewUserProfile profile, Action<bool, string> onComplete)
    {
        string url = $"{supabaseUrl}/rest/v1/users";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(profile));
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            request.SetRequestHeader("Prefer", "return=minimal");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) onComplete?.Invoke(true, null);
            else onComplete?.Invoke(false, ExtractErrorMessage(request));
        }
    }
    public void GetUserProfile(string userId, Action<bool, UserModel> onComplete)
    {
        StartCoroutine(GetUserProfileRoutine(userId, onComplete));
    }
    private IEnumerator GetUserProfileRoutine(string userId, Action<bool, UserModel> onComplete)
    {
        string url = $"{supabaseUrl}/rest/v1/users?select=*&id=eq.{userId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                ExtractErrorMessage(request);
                onComplete?.Invoke(false, null);
                yield break;
            }
            string rawJson = request.downloadHandler.text.Trim();
            if (rawJson == "[]" || string.IsNullOrEmpty(rawJson))
            {
                onComplete?.Invoke(false, null);
                yield break;
            }
            UserListWrapper wrapper = JsonUtility.FromJson<UserListWrapper>("{\"users\":" + rawJson + "}");
            if (wrapper != null && wrapper.users != null && wrapper.users.Length > 0) onComplete?.Invoke(true, wrapper.users[0]);
            else onComplete?.Invoke(false, null);
        }
    }
    public void GetUserByUsernameOrEmail(string usernameOrEmail, Action<bool, UserModel> onComplete)
    {
        StartCoroutine(GetUserByUsernameOrEmailRoutine(usernameOrEmail, onComplete));
    }
    private IEnumerator GetUserByUsernameOrEmailRoutine(string usernameOrEmail, Action<bool, UserModel> onComplete)
    {
        string escaped = UnityWebRequest.EscapeURL(usernameOrEmail);
        string url = $"{supabaseUrl}/rest/v1/users?select=*&or=(username.eq.{escaped},email.eq.{escaped})";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                ExtractErrorMessage(request);
                onComplete?.Invoke(false, null);
                yield break;
            }
            string rawJson = request.downloadHandler.text.Trim();
            if (rawJson == "[]" || string.IsNullOrEmpty(rawJson))
            {
                onComplete?.Invoke(false, null);
                yield break;
            }
            UserListWrapper wrapper = JsonUtility.FromJson<UserListWrapper>("{\"users\":" + rawJson + "}");
            if (wrapper != null && wrapper.users != null && wrapper.users.Length > 0) onComplete?.Invoke(true, wrapper.users[0]);
            else onComplete?.Invoke(false, null);
        }
    }
    public void UpdateUserProfile(string userId, string photoUrl, Action<bool, string> onComplete)
    {
        StartCoroutine(UpdateUserProfileRoutine(userId, photoUrl, onComplete));
    }
    private IEnumerator UpdateUserProfileRoutine(string userId, string photoUrl, Action<bool, string> onComplete)
    {
        string url = $"{supabaseUrl}/rest/v1/users?id=eq.{userId}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes("{\"photo_url\":\"" + photoUrl + "\"}");
        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) onComplete?.Invoke(true, null);
            else onComplete?.Invoke(false, ExtractErrorMessage(request));
        }
    }
    public void UploadProfileImage(string userId, string fileName, byte[] fileData, Action<bool, string> onComplete)
    {
        UploadProfilePicture(fileName, fileData, (uploadSuccess, publicUrl) =>
        {
            if (uploadSuccess)
            {
                UpdateUserProfile(userId, publicUrl, (updateSuccess, error) =>
                {
                    if (updateSuccess) onComplete?.Invoke(true, publicUrl);
                    else onComplete?.Invoke(false, error);
                });
            }
            else onComplete?.Invoke(false, "Failed to upload image.");
        });
    }
    public void UploadProfilePicture(string fileName, byte[] fileData, Action<bool, string> onComplete)
    {
        StartCoroutine(UploadProfilePictureRoutine(fileName, fileData, onComplete));
    }
    private IEnumerator UploadProfilePictureRoutine(string fileName, byte[] fileData, Action<bool, string> onComplete)
    {
        string bucket = "avatars";
        string url = $"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(fileData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "image/png");
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) onComplete?.Invoke(true, $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}");
            else onComplete?.Invoke(false, ExtractErrorMessage(request));
        }
    }
    private UnityWebRequest BuildPostRequest(string url, string json, bool includeAuthHeader)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);
        if (includeAuthHeader && GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.AccessToken))
        {
            request.SetRequestHeader("Authorization", $"Bearer {GameManager.Instance.AccessToken}");
        }
        return request;
    }
    private void HandleAuthResponse(UnityWebRequest request, Action<bool, string, AuthResponse> onComplete)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            onComplete?.Invoke(true, null, JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text));
        }
        else onComplete?.Invoke(false, ExtractErrorMessage(request), null);
    }
    private string ExtractErrorMessage(UnityWebRequest request)
    {
        string rawBody = request.downloadHandler != null ? request.downloadHandler.text : "";
        if (request.responseCode == 0) return "Connection failed. Please check internet connection or URL settings.";
        try
        {
            if (!string.IsNullOrEmpty(rawBody))
            {
                SupabaseErrorResponse error = JsonUtility.FromJson<SupabaseErrorResponse>(rawBody);
                if (error != null)
                {
                    if (!string.IsNullOrEmpty(error.msg)) return error.msg;
                    if (!string.IsNullOrEmpty(error.message)) return error.message;
                    if (!string.IsNullOrEmpty(error.error_description)) return error.error_description;
                }
            }
        }
        catch { }
        return "Something went wrong. Please try again.";
    }
}