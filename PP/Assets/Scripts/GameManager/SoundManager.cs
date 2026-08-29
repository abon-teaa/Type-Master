using UnityEngine;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public AudioSource sfxSource;
    public AudioClip correctSound, wrongSound, explosionSound;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    public void PlayCorrectSound()
    {
        if (sfxSource && correctSound) sfxSource.PlayOneShot(correctSound);
    }
    public void PlayWrongSound()
    {
        if (sfxSource && wrongSound) sfxSource.PlayOneShot(wrongSound);
    }
    public void PlayExplosionSound()
    {
        if (sfxSource && explosionSound) sfxSource.PlayOneShot(explosionSound);
    }
}