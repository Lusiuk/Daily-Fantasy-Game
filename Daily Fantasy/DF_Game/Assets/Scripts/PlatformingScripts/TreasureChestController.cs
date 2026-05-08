using UnityEngine;
using UnityEngine.InputSystem;

public class TreasureChestController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private InputActionReference interactAction;

    [Header("Treasure UI")]
    [SerializeField] private GameObject treasurePrefab;
    [SerializeField] private Transform treasureParent;

    private GameObject treasureInstance;
    private bool playerInRange;
    private bool isOpen;

    private void Start()
    {
        if (dialogueTrigger != null)
        {
            dialogueTrigger.SetInteractionEnabled(false);
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteract;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
         CloseChest();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerInRange) return;
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive()) return;

        if (!isOpen)
        {
            // ОТКРЫТЬ сундук
            OpenChest();
        }
        else
        {
            // ЗАКРЫТЬ сундук
            CloseChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true;

        if (chestAnimator != null)
            chestAnimator.SetTrigger("Open");

        if (treasurePrefab != null)
        {
            treasureInstance = Instantiate(treasurePrefab, treasureParent);
            RectTransform canvasRect = treasureInstance.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.offsetMin = Vector2.zero;
                canvasRect.offsetMax = Vector2.zero;
            }
            treasureInstance.SetActive(true);
        }

        if (dialogueTrigger != null)
        {
            dialogueTrigger.TriggerDialogue();
        }
    }

    private void CloseChest()
    {
        // Удаляем canvas
        if (treasureInstance != null)
        {
            Destroy(treasureInstance);
            treasureInstance = null;
        }

        // Анимация закрытия
        if (chestAnimator != null)
            chestAnimator.SetTrigger("Close");

        isOpen = false;
    }
}