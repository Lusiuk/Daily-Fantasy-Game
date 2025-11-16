using UnityEngine;

// CreateAssetMenu позволяет создавать объекты диалога через меню Unity
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    // TextArea создает многострочное текстовое поле в инспекторе
    [TextArea(3, 5)]
    public string text;

    // Можно ли взаимодействовать с этим диалогом
    public bool canInteract = true;
}