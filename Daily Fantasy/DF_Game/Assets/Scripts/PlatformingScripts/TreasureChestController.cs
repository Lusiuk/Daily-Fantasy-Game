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
    private bool canClose;

    private void Awake()
    {
        if (treasureParent == null)
            treasureParent = null; // можно оставить null: будет инстанс в корне
    }

    private void Start()
    {
        if (dialogueTrigger != null)
            dialogueTrigger.SetInteractionEnabled(false); // чтобы R обрабатывал только этот скрипт

        if (treasurePrefab != null)
        {
            treasureInstance = Instantiate(treasurePrefab, treasureParent);
            treasureInstance.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteract;
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.OnDialogueEnd += OnDialogueEnd;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteract;
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.OnDialogueEnd -= OnDialogueEnd;
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
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerInRange) return;
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive()) return;

        if (!isOpen)
        {
            OpenChest();
        }
        else if (canClose)
        {
            CloseChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true;
        canClose = false;

        if (chestAnimator != null)
            chestAnimator.SetTrigger("Open");

        if (treasureInstance != null)
            treasureInstance.SetActive(true);

        if (dialogueTrigger != null)
            dialogueTrigger.TriggerDialogue();
    }

    private void CloseChest()
    {
        if (treasureInstance != null)
            treasureInstance.SetActive(false);

        if (chestAnimator != null)
            chestAnimator.SetTrigger("Close");

        isOpen = false;
        canClose = false;
    }

    private void OnDialogueEnd()
    {
        if (DialogueSystem.Instance == null) return;
        if (DialogueSystem.Instance.CurrentContextObject != gameObject) return;

        // диалог сундука закрыт — разрешаем закрыть сундук на следующий R
        canClose = true;
    }
}