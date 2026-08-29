using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    public AudioSource sfxSource;        

    [Header("Sound Clips")]
    public AudioClip correctSound;       
    public AudioClip wrongSound;
    public AudioClip explosionSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayCorrectSound()
    {
        if (sfxSource != null && correctSound != null)
            sfxSource.PlayOneShot(correctSound);
        else
            Debug.LogWarning("SFX Source or Correct Sound is missing!");
    }

    public void PlayWrongSound()
    {
        if (sfxSource != null && wrongSound != null)
            sfxSource.PlayOneShot(wrongSound);
        else
            Debug.LogWarning("SFX Source or Wrong Sound is missing!");
    }
    public void PlayExplosionSound()
    {
        if (sfxSource != null && explosionSound != null)
            sfxSource.PlayOneShot(explosionSound);
        else
            Debug.LogWarning("SFX Source or Explosion Sound is missing!");
    }
}