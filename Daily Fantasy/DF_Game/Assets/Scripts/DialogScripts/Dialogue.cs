using UnityEngine;

// Позволяет создавать объекты диалога через меню Unity
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    // Создаёт многострочное текстовое поле в инспекторе
    [TextArea(3, 5)]
    public string text;

    // Определяет можно ли взаимодействовать с этим диалогом
    public bool canInteract = true;

    [Header("Scene Transition Settings")]
    public bool triggerSceneTransition = false; // Включает переход сцены после диалога
    public string targetSceneName; // Имя сцены для перехода
}