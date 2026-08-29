using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance { get; private set; }
    [Header("Supabase Project Settings")]
    [Tooltip("Your Supabase project URL, e.g. https://xxxxx.supabase.co")]
    public string supabaseUrl = "https://YOUR_PROJECT_ID.supabase.co";
    [Tooltip("Your Supabase 'anon' public API key (Project Settings > API)")]
    public string supabaseAnonKey = "YOUR_ANON_PUBLIC_KEY";
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SignUp(string email, string password, Action<bool, string, AuthResponse> onComplete)
    {
        StartCoroutine(SignUpRoutine(email, password, onComplete));
    }
    private IEnumerator SignUpRoutine(string email, string password, Action<bool, string, AuthResponse> onComplete)
    {
        string url = $"{supabaseUrl}/auth/v1/signup";
        SignUpRequest body = new SignUpRequest { email = email, password = password };
        string json = JsonUtility.ToJson(body);
        using (UnityWebRequest request = BuildPostRequest(url, json, includeAuthHeader: false))
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
        string json = JsonUtility.ToJson(body);
        using (UnityWebRequest request = BuildPostRequest(url, json, includeAuthHeader: false))
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
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {GameManager.Instance.AccessToken}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                onComplete?.Invoke(true, null);
            else
                onComplete?.Invoke(false, ExtractErrorMessage(request));
        }
    }
    public void UpdatePassword(string accessToken, string newPassword, Action<bool, string> onComplete)
    {
        StartCoroutine(UpdatePasswordRoutine(accessToken, newPassword, onComplete));
    }
    private IEnumerator UpdatePasswordRoutine(string accessToken, string newPassword, Action<bool, string> onComplete)
    {
        string url = $"{supabaseUrl}/auth/v1/user";
        UpdatePasswordRequest body = new UpdatePasswordRequest { password = newPassword };
        string json = JsonUtility.ToJson(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                onComplete?.Invoke(true, null);
            else
                onComplete?.Invoke(false, ExtractErrorMessage(request));
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
                onComplete?.Invoke(false, false);
                yield break;
            }
            string responseText = request.downloadHandler.text.Trim();
            bool isAvailable = responseText == "[]";
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
        string json = JsonUtility.ToJson(profile);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            request.SetRequestHeader("Prefer", "return=minimal");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                onComplete?.Invoke(true, null);
            else
                onComplete?.Invoke(false, ExtractErrorMessage(request));
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
                onComplete?.Invoke(false, null);
                yield break;
            }
            string wrapped = "{\"users\":" + request.downloadHandler.text + "}";
            UserListWrapper wrapper = JsonUtility.FromJson<UserListWrapper>(wrapped);
            if (wrapper.users != null && wrapper.users.Length > 0)
                onComplete?.Invoke(true, wrapper.users[0]);
            else
                onComplete?.Invoke(false, null);
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
                onComplete?.Invoke(false, null);
                yield break;
            }
            string wrapped = "{\"users\":" + request.downloadHandler.text + "}";
            UserListWrapper wrapper = JsonUtility.FromJson<UserListWrapper>(wrapped);
            if (wrapper.users != null && wrapper.users.Length > 0)
                onComplete?.Invoke(true, wrapper.users[0]);
            else
                onComplete?.Invoke(false, null);
        }
    }
    public void UpdateUserProfile(string userId, string photoUrl, Action<bool, string> onComplete)
    {
        StartCoroutine(UpdateUserProfileRoutine(userId, photoUrl, onComplete));
    }
    private IEnumerator UpdateUserProfileRoutine(string userId, string photoUrl, Action<bool, string> onComplete)
    {
        string url = $"{supabaseUrl}/rest/v1/users?id=eq.{userId}";
        string json = "{\"photo_url\":\"" + photoUrl + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                onComplete?.Invoke(true, null);
            else
                onComplete?.Invoke(false, ExtractErrorMessage(request));
        }
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
            if (request.result == UnityWebRequest.Result.Success)
            {
                string publicUrl = $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}";
                onComplete?.Invoke(true, publicUrl);
            }
            else
            {
                onComplete?.Invoke(false, ExtractErrorMessage(request));
            }
        }
    }
    private UnityWebRequest BuildPostRequest(string url, string json, bool includeAuthHeader)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);
        if (includeAuthHeader)
            request.SetRequestHeader("Authorization", $"Bearer {GameManager.Instance.AccessToken}");
        return request;
    }
    private void HandleAuthResponse(UnityWebRequest request, Action<bool, string, AuthResponse> onComplete)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
            onComplete?.Invoke(true, null, response);
        }
        else
        {
            onComplete?.Invoke(false, ExtractErrorMessage(request), null);
        }
    }
    private string ExtractErrorMessage(UnityWebRequest request)
    {
        try
        {
            string body = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(body))
            {
                SupabaseErrorResponse error = JsonUtility.FromJson<SupabaseErrorResponse>(body);
                if (!string.IsNullOrEmpty(error.msg)) return error.msg;
                if (!string.IsNullOrEmpty(error.message)) return error.message;
                if (!string.IsNullOrEmpty(error.error_description)) return error.error_description;
            }
        }
        catch
        {
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
            return "No internet connection. Please check your network and try again.";
        return "Something went wrong. Please try again.";
    }
}
