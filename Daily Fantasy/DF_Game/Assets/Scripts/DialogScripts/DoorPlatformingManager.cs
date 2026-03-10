using UnityEngine;

public class DoorPlatformingManager : MonoBehaviour
{
    [Header("Door Configuration")]
    [SerializeField] private int doorNumber = 1;
    [SerializeField] private DialogueTrigger doorTrigger;

    [Header("Dialogs")]
    [SerializeField] private Dialogue lockedDialog;
    [SerializeField] private Dialogue availableDialog;
    [SerializeField] private Dialogue completedDialog;

    void Start()
    {
        if (doorTrigger == null)
            doorTrigger = GetComponent<DialogueTrigger>();

        UpdateDoorState();
    }

    void UpdateDoorState()
    {
        bool canAccess = CanAccessDoor();
        bool isCompleted = IsDoorCompleted();

        if (isCompleted)
        {
            SetDoorDialogue(completedDialog);
            EnableDoorInteraction();
        }
        else if (canAccess)
        {
            SetDoorDialogue(availableDialog);
            EnableDoorInteraction();
        }
        else
        {
            SetDoorDialogue(lockedDialog);
            EnableDoorInteraction();
        }
    }

    bool CanAccessDoor()
    {
        switch (doorNumber)
        {
            case 1: return true;
            case 2: return GameState.IsDoor1Completed;
            case 3: return GameState.IsDoor2Completed;
            default: return false;
        }
    }

    bool IsDoorCompleted()
    {
        switch (doorNumber)
        {
            case 1: return GameState.IsDoor1Completed;
            case 2: return GameState.IsDoor2Completed;
            case 3: return GameState.IsDoor3Completed;
            default: return false;
        }
    }

    void SetDoorDialogue(Dialogue newDialogue)
    {
        if (doorTrigger != null && newDialogue != null)
        {
            var field = typeof(DialogueTrigger).GetField("dialogue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(doorTrigger, newDialogue);
                Debug.Log($"Дверь {doorNumber}: установлен диалог {newDialogue.name}");
            }
        }
    }

    void EnableDoorInteraction()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
            Debug.Log($"Дверь {doorNumber}: коллайдер включен");
        }

        if (doorTrigger != null)
        {
            doorTrigger.enabled = true;
            Debug.Log($"Дверь {doorNumber}: триггер включен");
        }
    }

    // Вызывается из мини-игры при успешном прохождении
    public static void CompleteDoor(int doorNumber)
    {
        switch (doorNumber)
        {
            case 1:
                GameState.IsDoor1Completed = true;
                Debug.Log("Дверь 1 пройдена!");
                break;
            case 2:
                GameState.IsDoor2Completed = true;
                Debug.Log("Дверь 2 пройдена!");
                break;
            case 3:
                GameState.IsDoor3Completed = true;
                Debug.Log("Дверь 3 пройдена! Все двери пройдены!");
                break;
        }
    }
}