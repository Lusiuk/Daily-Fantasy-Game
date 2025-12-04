using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool autoTrigger = false;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float promptOffset = 4f;
    [SerializeField] private float uiAdditionalOffset = 90f;

    // Приватные переменные
    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private Camera mainCamera;
    private RectTransform promptRectTransform;
    private bool isActive = true;

    // Инициализирует компоненты при старте
    void Start()
    {
        mainCamera = Camera.main;

        if (interactionPrompt != null)
        {
            promptRectTransform = interactionPrompt.GetComponent<RectTransform>();

            interactionPrompt.SetActive(false);
        }
    }

    // Настраивает систему ввода при активации
    void OnEnable()
    {
        isActive = true;

        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    // Отключает систему ввода при деактивации
    void OnDisable()
    {
        isActive = false;

        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
    }

    // Обрабатывает вход игрока в триггер
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;

            if (interactionPrompt != null && !hasBeenUsed)
            {
                interactionPrompt.SetActive(true);
                UpdatePromptPosition();
            }

            if (autoTrigger && !hasBeenUsed)
            {
                TriggerDialogue();
            }
        }
    }

    // Обрабатывает выход игрока из триггера
    void OnTriggerExit2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    // Обновляет позицию подсказки
    void Update()
    {
        if (!isActive || !playerInRange || interactionPrompt == null || !interactionPrompt.activeSelf)
            return;

        UpdatePromptPosition();
    }

    // Обрабатывает нажатие клавиши взаимодействия
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!isActive || !gameObject.activeInHierarchy) return;

        if (playerInRange && !autoTrigger && !hasBeenUsed && DialogueSystem.Instance != null && !DialogueSystem.Instance.IsDialogueActive())
        {
            TriggerDialogue();
        }
    }

    // Обновляет позицию подсказки над игроком
    private void UpdatePromptPosition()
    {
        if (playerTransform != null && mainCamera != null)
        {
            Vector3 worldPosition = playerTransform.position + Vector3.up * promptOffset;
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += uiAdditionalOffset;

            if (promptRectTransform != null)
            {
                promptRectTransform.position = screenPosition;
            }
            else
            {
                interactionPrompt.transform.position = worldPosition;
            }
        }
    }

    // Запускает диалог
    public void TriggerDialogue()
    {
        if (!isActive) return;

        if (dialogue == null || hasBeenUsed || DialogueSystem.Instance == null) return;

        DialogueSystem.Instance.ShowDialogue(dialogue);

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }
    }

    // Сбрасывает состояние триггера
    public void ResetTrigger()
    {
        hasBeenUsed = false;

        if (interactionPrompt != null && playerInRange)
        {
            interactionPrompt.SetActive(true);
        }
    }
}