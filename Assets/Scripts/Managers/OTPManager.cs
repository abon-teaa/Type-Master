using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class OTPManager : MonoBehaviour
{
    public static OTPManager Instance { get; private set; }
    [Header("Google Apps Script Web App URL")]
    [Tooltip("The deployed /exec URL from Google Apps Script (see deployment instructions)")]
    public string appsScriptUrl = "https://script.google.com/macros/s/YOUR_DEPLOYMENT_ID/exec";
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
    public void SendOtp(string email, Action<bool, string> onComplete)
    {
        string json = "{\"action\":\"send\",\"email\":\"" + email + "\"}";
        StartCoroutine(PostToAppsScript(json, onComplete));
    }
    public void VerifyOtp(string email, string otp, Action<bool, string> onComplete)
    {
        string json = "{\"action\":\"verify\",\"email\":\"" + email + "\",\"otp\":\"" + otp + "\"}";
        StartCoroutine(PostToAppsScript(json, onComplete));
    }
    private IEnumerator PostToAppsScript(string jsonBody, Action<bool, string> onComplete)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        using (UnityWebRequest request = new UnityWebRequest(appsScriptUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OTP request failed: " + request.result + " | " + request.error + " | Response code: " + request.responseCode);
                onComplete?.Invoke(false, "No internet connection. Please try again.");
                yield break;
            }

            OtpResponse response = JsonUtility.FromJson<OtpResponse>(request.downloadHandler.text);
            onComplete?.Invoke(response.success, response.message);
        }
    }
    [Serializable]
    private class OtpResponse
    {
        public bool success;
        public string message;
    }
}
