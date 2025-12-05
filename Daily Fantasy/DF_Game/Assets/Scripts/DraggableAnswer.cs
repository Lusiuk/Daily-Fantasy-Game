using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DraggableAnswer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Graphic raycastGraphic;

    private Transform originalParent;

     public string codeLine;
    public int originalIndex = -1;

    private TMP_Text textComponent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = rectTransform.parent;
        rectTransform.SetParent(transform.root);
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        AnswerSlot slot = GetDropSlot(eventData);
        if (slot != null && slot.transform.childCount == 0)
        {
            parentAfterDrag = slot.transform;
            slot.OnAnswerDropped(this);
        }
        rectTransform.SetParent(parentAfterDrag);

    }

    private AnswerSlot GetDropSlot(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Проверяем, является ли объект или его родитель AnswerSlot
            AnswerSlot slot = result.gameObject.GetComponentInParent<AnswerSlot>();
            if (slot != null)
            {
                // Проверяем, активен ли Raycast Target для графики (Image/Text)
                Graphic graphic = result.gameObject.GetComponent<Graphic>();
                if (graphic == null || graphic.raycastTarget)
                {
                    return slot;
                }
            }
        }
        return null;
    }

    public void Initialize(string code, int index)
    {
        codeLine = code;
        if (textComponent != null)
        {
            textComponent.text = code;
        }
        originalIndex = index;
    }

     public void ResetPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero; 
        rectTransform.localScale = Vector3.one;
        transform.SetAsLastSibling();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalParent = transform.parent;
        rectTransform = GetComponent<RectTransform>();
        canvas = Object.FindFirstObjectByType<Canvas>();
        raycastGraphic = GetComponent<Graphic>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
