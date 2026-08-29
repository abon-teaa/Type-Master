using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    public float floatSpeed = 2f;      
    public float fadeSpeed = 2f;       
    public float destroyDelay = 1f;    

    private TextMeshProUGUI text;
    private Color color;

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
