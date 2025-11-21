using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DraggableCodeBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string codeLine;
    public int originalIndex = -1;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private TMP_Text textComponent;
    
    private Transform originalParent;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = Object.FindFirstObjectByType<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        textComponent = GetComponent<TMP_Text>();
        
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
        originalParent = transform.parent;
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

    // IBeginDragHandler
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Проверяем, находится ли блок уже в зоне сборки
        bool isInBuildZone = transform.parent.CompareTag("DropZone");
        
        if (isInBuildZone)
        {
            // Если блок уже в зоне сборки, запрещаем перетаскивание
            return;
        }
        
        //canvasGroup.alpha = 0.7f;
        //canvasGroup.blocksRaycasts = false;
        originalParent = transform.parent;
        
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
    }

    // IDragHandler
    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || canvas == null) return;
        
        // Важно: делим на scaleFactor для корректного перемещения
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // IEndDragHandler
    public void OnEndDrag(PointerEventData eventData)
    {
        //canvasGroup.alpha = 1f;
        //canvasGroup.blocksRaycasts = true;

        // Проверяем, где отпустили блок
        Transform dropZone = null;
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            dropZone = eventData.pointerCurrentRaycast.gameObject.transform;
            // Ищем родительскую зону сброса
            while (dropZone != null && !dropZone.CompareTag("DropZone"))
            {
                dropZone = dropZone.parent;
            }
        }

        bool droppedInBuildZone = dropZone != null && dropZone.CompareTag("DropZone");

        if (!droppedInBuildZone)
        {
            // Возвращаем на исходную позицию
            ResetPosition();
        }
        else
        {
            // Перемещаем в зону сборки
            transform.SetParent(dropZone);
            // Автоматически позиционируем в правильном месте списка
            RepositionInBuildZone(eventData.position);
        }
    }
    
    private void RepositionInBuildZone(Vector2 dragPosition)
    {
        // Получаем все блоки в зоне сборки
        var blocksInZone = new List<DraggableCodeBlock>();
        foreach (Transform child in transform.parent)
        {
            if (child != transform) // пропускаем текущий блок
            {
                DraggableCodeBlock block = child.GetComponent<DraggableCodeBlock>();
                if (block != null)
                {
                    blocksInZone.Add(block);
                }
            }
        }

        // Сортируем по позиции Y
        blocksInZone.Sort((a, b) => 
            a.rectTransform.anchoredPosition.y.CompareTo(b.rectTransform.anchoredPosition.y)
        );

        // Находим позицию для вставки
        int insertIndex = blocksInZone.Count;
        for (int i = 0; i < blocksInZone.Count; i++)
        {
            if (dragPosition.y > blocksInZone[i].rectTransform.position.y)
            {
                insertIndex = i;
                break;
            }
        }

        // Устанавливаем на правильную позицию в иерархии
        transform.SetSiblingIndex(insertIndex);
    }

    // Для сброса уровня
    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        
        if (rectTransform != null && originalPosition != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}