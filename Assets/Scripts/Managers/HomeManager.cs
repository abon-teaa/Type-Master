using UnityEngine;
public class HomeManager : MonoBehaviour
{
    public void OnLoginButtonPressed()
    {
        SceneLoader.LoadScene("LoginScene");
    }
    public void OnRegisterButtonPressed()
    {
        SceneLoader.LoadScene("RegisterScene");
    }
}
