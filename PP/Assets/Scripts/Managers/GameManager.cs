using UnityEngine;
public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public string AccessToken, RefreshToken, PendingEmail, PendingUsername;
    public UserModel CurrentUser;
    public static GameManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindAnyObjectByType<GameManager>();
                if (!_instance) _instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return _instance;
        }
    }
    void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
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