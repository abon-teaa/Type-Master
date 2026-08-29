using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string AccessToken { get; set; }   
    public string RefreshToken { get; set; }
    public UserModel CurrentUser { get; set; } 
    public string PendingEmail { get; set; }     
    public string PendingUsername { get; set; }  
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
    public void ClearSession()
    {
        AccessToken = null;
        RefreshToken = null;
        CurrentUser = null;
        PendingEmail = null;
        PendingUsername = null;
    }
    public bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(AccessToken) && CurrentUser != null;
    }
}
