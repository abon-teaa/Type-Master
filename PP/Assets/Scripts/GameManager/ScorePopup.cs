using UnityEngine;
using TMPro;
public class ScorePopup : MonoBehaviour
{
    public float floatSpeed = 2f, fadeSpeed = 2f, destroyDelay = 1f;
    TextMeshProUGUI text;
    Color color;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        color = text.color;
        Destroy(gameObject, destroyDelay);
    }
    void Update()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        color.a -= fadeSpeed * Time.deltaTime;
        text.color = color;
    }
}