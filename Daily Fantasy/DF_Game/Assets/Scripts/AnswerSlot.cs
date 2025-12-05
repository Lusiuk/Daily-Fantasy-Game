using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnswerSlot : MonoBehaviour
{

    public void OnAnswerDropped(DraggableAnswer answer)
    {
        Debug.Log($"Answer '{answer.name}' dropped into slot '{name}'");
    }

    // Для активации Raycast Target (даже если слот невидим)
    void Start()
    {
        // Если нет визуального элемента (Image/Text), добавляем пустой
        if (GetComponent<Graphic>() == null)
        {
            gameObject.AddComponent<Image>().color = Color.clear;
        }
    }
}