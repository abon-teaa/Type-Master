using UnityEngine;
using TMPro;

public class BombBehaviour : MonoBehaviour
{
    public TextMeshPro wordText;      
    public GameObject explosionPrefab; 

    public float maxGravity = 0.8f;             
    public float minGravity = 0.09f;            
    public float gravityReductionPerLetter = 0.18f; 

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetWord(string newWord)
    {
        if (wordText != null)
        {
            wordText.text = newWord;
        }
        int letterCount = newWord.Length;
        float targetGravity = maxGravity - (letterCount * gravityReductionPerLetter);

        if (rb != null)
        {
            rb.gravityScale = Mathf.Max(targetGravity, minGravity);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayExplosionSound();

            if (explosionPrefab != null)
            {
                Vector3 contactPoint = collision.GetContact(0).point;
                Instantiate(explosionPrefab, contactPoint, Quaternion.identity);
            }

            if (HeartManager.Instance != null)
            {
                HeartManager.Instance.LoseHeart();
            }

            Destroy(gameObject);
        }
    }
}