using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool autoTrigger = false;

    [Header("Minigame Settings")]
    [SerializeField] private bool isMinigameTrigger = false;
    [SerializeField] private string minigameName = "IDEMinigame";

    [Header("Input Settings")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float promptOffset = 4f;
    [SerializeField] private float uiAdditionalOffset = 90f;
    [SerializeField] private float smoothTime = 0.1f;

    // Приватные переменные
    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private Camera mainCamera;
    private RectTransform promptRectTransform;
    private bool isActive = true;
    private Vector2 targetScreenPosition;
    private Vector2 currentVelocity = Vector2.zero;

    // Инициализирует компоненты при старте
    void Start()
    {
        mainCamera = Camera.main;

        if (interactionPrompt != null)
        {
            promptRectTransform = interactionPrompt.GetComponent<RectTransform>();

            interactionPrompt.SetActive(false);
        }

        // Проверяем состояние мини-игры
        if (isMinigameTrigger && GameState.IsMinigameCompleted && GameState.MinigameName == minigameName)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
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

        if (isMinigameTrigger && GameState.IsMinigameCompleted && GameState.MinigameName == minigameName)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;

            if (interactionPrompt != null && !hasBeenUsed)
            {
                interactionPrompt.SetActive(true);
                SnapPromptPosition();
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
                currentVelocity = Vector2.zero;
                interactionPrompt.SetActive(false);
                targetScreenPosition = Vector2.zero;
            }
        }
    }

    // Обновляет позицию подсказки
    void Update()
    {
        if (!isActive || !playerInRange || interactionPrompt == null || !interactionPrompt.activeSelf)
            return;

        UpdatePromptPositionSmooth();
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

    // Плавно обновляет позицию подсказки над игроком
    private void UpdatePromptPositionSmooth()
    {
        if (playerTransform != null && mainCamera != null)
        {
            // Вычисляем новую позицию
            Vector3 worldPosition = playerTransform.position + Vector3.up * promptOffset;
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += uiAdditionalOffset;

            // Обновляем только если позиция изменилась больше чем на 2 пикселя
            if (Vector2.Distance(targetScreenPosition, screenPosition) > 2f)
            {
                targetScreenPosition = screenPosition;
            }

            if (promptRectTransform != null)
            {
                // Для более плавного движения
                Vector2 currentPosition = promptRectTransform.position;
                Vector2 smoothedPosition = Vector2.SmoothDamp(
                    currentPosition,
                    targetScreenPosition,
                    ref currentVelocity,
                    smoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime
                );
                promptRectTransform.position = smoothedPosition;
            }
        }
    }

    // Мгновенная установка позиции
    private void SnapPromptPosition()
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

            targetScreenPosition = screenPosition;
            currentVelocity = Vector2.zero;
        }
    }

    // Запускает диалог
    public void TriggerDialogue()
    {
        if (!isActive) return;

        if (isMinigameTrigger && GameState.IsMinigameCompleted && GameState.MinigameName == minigameName)
        {
            return;
        }

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
            SnapPromptPosition();
        }
    }
}