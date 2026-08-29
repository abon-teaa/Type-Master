using UnityEngine;
public class CharacterAnimator : MonoBehaviour
{
    public static CharacterAnimator Instance { get; private set; }
    Animator anim;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        anim = GetComponent<Animator>();
    }
    public void PlayAction()
    {
        if (anim) anim.SetTrigger("Action");
    }
    public void PlayJoy()
    {
        if (anim) anim.SetTrigger("Joy");
    }
    public void PlaySad()
    {
        if (anim) anim.SetTrigger("Sad");
    }
}