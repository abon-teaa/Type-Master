using UnityEngine;
using TMPro;
public class BombBehaviour : MonoBehaviour
{
    public TextMeshPro wordText;
    public GameObject explosionPrefab;
    public float maxGravity = 0.8f, minGravity = 0.09f, gravityReductionPerLetter = 0.18f;
    Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetWord(string newWord)
    {
        if (wordText)
        {
            wordText.text = newWord;
            if (SettingsManager.Instance) wordText.color = SettingsManager.Instance.GetCurrentTextColor();
        }
        var letterCount = newWord.Length;
        var targetGravity = maxGravity - (letterCount * gravityReductionPerLetter);
        if (rb) rb.gravityScale = Mathf.Max(targetGravity, minGravity);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            if (CharacterAnimator.Instance) CharacterAnimator.Instance.PlaySad();
            if (ScoreManager.Instance) ScoreManager.Instance.ResetCombo();
            if (SoundManager.Instance) SoundManager.Instance.PlayExplosionSound();
            if (explosionPrefab)
            {
                var contactPoint = collision.GetContact(0).point;
                Instantiate(explosionPrefab, contactPoint, Quaternion.identity);
            }
            if (HeartManager.Instance) HeartManager.Instance.LoseHeart();
            Destroy(gameObject);
        }
    }
}