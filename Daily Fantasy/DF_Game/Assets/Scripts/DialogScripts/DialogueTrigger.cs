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
    [SerializeField] private float fadePromptSpeed = 5f;

    // Приватные переменные
    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private Camera mainCamera;
    private RectTransform promptRectTransform;
    private bool isActive = true;
    private Vector2 targetScreenPosition;
    private Vector2 currentVelocity = Vector2.zero;
    private CanvasGroup promptCanvasGroup;
    private bool isPromptVisible = false;
    private bool dialogueTriggeredThisSession = false;

    // Инициализирует компоненты при старте
    void Start()
    {
        mainCamera = Camera.main;

        if (interactionPrompt != null)
        {
            promptRectTransform = interactionPrompt.GetComponent<RectTransform>();

            // Добавляем или получаем CanvasGroup
            promptCanvasGroup = interactionPrompt.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = interactionPrompt.AddComponent<CanvasGroup>();
            }

            // Подсказка всегда активна, но прозрачна
            interactionPrompt.SetActive(true);
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;

            isPromptVisible = false;
        }

        // Проверяем состояние мини-игры
        if (isMinigameTrigger && GameState.IsIDEMinigameCompleted && GameState.MinigameName == minigameName)
        {
            if (interactionPrompt != null)
            {
                // Делаем подсказку полностью невидимой и отключаем
                promptCanvasGroup.alpha = 0f;
                interactionPrompt.SetActive(false);
            }

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        CheckAndUpdateDialogue();
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

        if (isMinigameTrigger && GameState.IsIDEMinigameCompleted && GameState.MinigameName == minigameName)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
            dialogueTriggeredThisSession = false;

            // Проверяем и обновляем диалог, затем показываем подсказку, если доступен
            CheckAndUpdateDialogue();
            UpdatePromptVisibility();

            if (autoTrigger && !hasBeenUsed && IsDialogueAvailable(dialogue))
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
            dialogueTriggeredThisSession = false;

            if (interactionPrompt != null)
            {
                HidePrompt();
                targetScreenPosition = Vector2.zero;
            }
        }
    }

    // Плавно показывает подсказку
    private void ShowPrompt()
    {
        if (promptCanvasGroup == null || isPromptVisible) return;

        isPromptVisible = true;
        StopAllCoroutines();
        StartCoroutine(FadePrompt(0f, 1f, fadePromptSpeed));
    }

    // Плавно скрывает подсказку
    private void HidePrompt()
    {
        if (promptCanvasGroup == null || !isPromptVisible) return;

        isPromptVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadePrompt(promptCanvasGroup.alpha, 0f, fadePromptSpeed));
    }

    // Корутина плавного изменения прозрачности
    private System.Collections.IEnumerator FadePrompt(float from, float to, float speed)
    {
        float elapsed = 0f;
        float duration = Mathf.Abs(to - from) / speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            if (promptCanvasGroup != null)
                promptCanvasGroup.alpha = alpha;
            yield return null;
        }

        if (promptCanvasGroup != null)
            promptCanvasGroup.alpha = to;
    }

    // Обновляет позицию подсказки
    void Update()
    {
        if (!isActive || !playerInRange || interactionPrompt == null || !isPromptVisible)
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
        if (playerTransform != null && mainCamera != null && isPromptVisible)
        {
            Vector3 worldPosition = playerTransform.position + Vector3.up * promptOffset;
            Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += uiAdditionalOffset;

            if (Vector2.Distance(targetScreenPosition, screenPosition) > 2f)
            {
                targetScreenPosition = screenPosition;
            }

            if (promptRectTransform != null)
            {
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
        if (isMinigameTrigger && GameState.IsIDEMinigameCompleted && GameState.MinigameName == minigameName) return;

        // Проверяем, доступен ли диалог и не использован ли
        if (dialogue == null || hasBeenUsed || DialogueSystem.Instance == null) return;
        if (!IsDialogueAvailable(dialogue)) return;

        DialogueSystem.Instance.ShowDialogue(dialogue);

        if (interactionPrompt != null) HidePrompt();

        dialogueTriggeredThisSession = true;

        if (oneTimeUse) hasBeenUsed = true;
    }

    // Сбрасывает состояние триггера
    public void ResetTrigger()
    {
        hasBeenUsed = false;

        if (interactionPrompt != null && playerInRange)
        {
            SnapPromptPosition();
            ShowPrompt();
        }
    }

    // Доступен ли диалог
    private bool IsDialogueAvailable(Dialogue d)
    {
        if (d == null) return false;
        return GameState.AreFlagsSatisfied(d.requiredFlags);
    }

    // Проверить и заменить диалог
    private void CheckAndUpdateDialogue()
    {
        if (hasBeenUsed || dialogue == null) return;

        Dialogue current = dialogue;
        bool changed = false;

        int maxIter = 10;
        while (maxIter-- > 0)
        {
            if (current.unlockedDialogue != null && GameState.AreFlagsSatisfied(current.unlockConditions))
            {
                current = current.unlockedDialogue;
                changed = true;
            }
            else
            {
                break;
            }
        }

        if (changed)
        {
            dialogue = current;
            Debug.Log($"{gameObject.name}: диалог заменён на {dialogue.name}");
        }
    }

    // Обновить видимость подсказки в зависимости от доступности диалога
    private void UpdatePromptVisibility()
    {
        bool available = IsDialogueAvailable(dialogue) && !hasBeenUsed && !dialogueTriggeredThisSession;

        if (available && playerInRange)
        {
            if (!isPromptVisible)
            {
                SnapPromptPosition();
                ShowPrompt();
            }
        }
        else
        {
            if (isPromptVisible) HidePrompt();
        }
    }
}