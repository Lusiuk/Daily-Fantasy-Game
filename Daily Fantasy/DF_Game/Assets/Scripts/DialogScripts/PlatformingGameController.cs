//using UnityEditor.EditorTools;
using UnityEngine;

public class PlatformingGameController : MonoBehaviour
{
    [Header("Door Configuration")]
    [SerializeField] private int doorNumber = 1;

    [Header("Completion Settings")]
    [SerializeField] private Dialogue completionDialogue;
    [SerializeField] private string returnSceneName = "CharacterRoom";
    [SerializeField] private Vector2 returnPosition;

    private bool isCompleted = false;

    void Start()
    {
        if (IsDoorCompleted())
        {
            Debug.Log("Эта дверь уже пройдена");
            ReturnToRoom();
        }
    }

    public void CompletePlatformingGame()
    {
        if (isCompleted) return;

        isCompleted = true;

        DoorPlatformingManager.CompleteDoor(doorNumber);

        if (completionDialogue != null && DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.ShowDialogue(completionDialogue);
        }

        Invoke("ReturnToRoom", 2f);
    }

    void ReturnToRoom()
    {
        if (TransitionManager.Instance != null)
        {
            GameState.ShouldTeleport = true;
            GameState.TeleportPosition = returnPosition;
            GameState.TeleportMarkerName = "";

            TransitionManager.Instance.TransitionToScene(returnSceneName);
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
}