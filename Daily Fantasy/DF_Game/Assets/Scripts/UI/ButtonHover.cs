using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Animator не найден на кнопке " + name);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (anim != null)
            anim.SetTrigger("hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (anim != null)
            anim.SetTrigger("idle");
    }
}