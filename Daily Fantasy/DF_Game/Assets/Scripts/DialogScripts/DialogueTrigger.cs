using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private bool autoTrigger = false;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float promptOffset = 4f;
    [SerializeField] private float uiAdditionalOffset = 90f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float fadePromptSpeed = 5f;

    [System.Serializable]
    public class DialogueReplacement
    {
        public string[] conditions;
        public Dialogue dialogue;
    }

    [Header("Dialogue Replacement")]
    [SerializeField] private DialogueReplacement[] replacements; // список замен

    [Header("Initial Visual Sync")]
    [SerializeField] private string syncFlagName;           // имя флага для проверки при старте
    [SerializeField] private Sprite onTrueSprite;           // спрайт, если флаг == true
    [SerializeField] private Sprite onFalseSprite;          // спрайт, если флаг == false
    [SerializeField] private bool modifyActive = false;     // изменять ли активность объекта
    [SerializeField] private bool activeWhenTrue = true;    // если modifyActive, то при true – такая активность
    [SerializeField] private GameObject targetObject;       // если не указан, действуем на gameObject

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
    private int appliedReplacementIndex = -1;

    // Инициализирует компоненты при старте
    void Start()
    {
        mainCamera = Camera.main;

        if (interactionPrompt != null)
        {
            promptRectTransform = interactionPrompt.GetComponent<RectTransform>();
            promptCanvasGroup = interactionPrompt.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = interactionPrompt.AddComponent<CanvasGroup>();
            }
            interactionPrompt.SetActive(true);
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
            isPromptVisible = false;
        }

        CheckAndApplyReplacement();

        // Проверка, не является ли текущий диалог
        if (dialogue != null && dialogue.oneTimeUse && !string.IsNullOrEmpty(dialogue.dialogueId) && GameState.IsDialogueUsed(dialogue.dialogueId))
        {
            DisableTrigger();
            return;
        }

        ApplyInitialVisual();

        UpdatePromptVisibility();
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

        GameState.OnFlagChanged += OnFlagChanged;
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

        GameState.OnFlagChanged -= OnFlagChanged;
    }

    private void OnFlagChanged(string flagName)
    {
        CheckAndApplyReplacement();

        if (playerInRange)
        {
            UpdatePromptVisibility();
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
            dialogueTriggeredThisSession = false;

            UpdatePromptVisibility();

            if (autoTrigger && !hasBeenUsed && dialogue != null)
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
        if (dialogue == null || hasBeenUsed || DialogueSystem.Instance == null) return;

        DialogueSystem.Instance.ShowDialogue(dialogue, gameObject);

        if (dialogue.oneTimeUse && !string.IsNullOrEmpty(dialogue.dialogueId))
        {
            GameState.MarkDialogueUsed(dialogue.dialogueId);
        }

        if (interactionPrompt != null) HidePrompt();
        dialogueTriggeredThisSession = true;
        if (dialogue.oneTimeUse) hasBeenUsed = true;
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

    private void CheckAndApplyReplacement()
    {
        if (replacements == null || replacements.Length == 0) return;

        // Ищем самую нижнюю подходящую замену, начиная с конца массива
        int bestIndex = -1;
        for (int i = replacements.Length - 1; i >= 0; i--)
        {
            var repl = replacements[i];
            if (repl.dialogue == null) continue;
            if (repl.conditions == null || repl.conditions.Length == 0) continue;

            if (GameState.AreFlagsSatisfied(repl.conditions))
            {
                bestIndex = i;
                break;
            }
        }

        if (bestIndex == -1) return;

        // Если это та же замена, что уже применена, ничего не делаем
        if (appliedReplacementIndex == bestIndex) return;

        // Применяем замену
        dialogue = replacements[bestIndex].dialogue;
        appliedReplacementIndex = bestIndex;
        Debug.Log($"{gameObject.name}: диалог заменён на {dialogue.name} (условия #{bestIndex})");

        // После замены перепроверяем доступность нового диалога
        if (dialogue != null && dialogue.oneTimeUse && !string.IsNullOrEmpty(dialogue.dialogueId) && GameState.IsDialogueUsed(dialogue.dialogueId))
        {
            DisableTrigger();
        }
        else
        {
            EnableTrigger();
            dialogueTriggeredThisSession = false;
            if (playerInRange)
            {
                UpdatePromptVisibility();
            }
        }
    }

    private void DisableTrigger()
    {
        if (interactionPrompt != null)
        {
            promptCanvasGroup.alpha = 0f;
            interactionPrompt.SetActive(false);
            isPromptVisible = false;
        }
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        hasBeenUsed = true;
    }

    private void EnableTrigger()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = true;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            promptCanvasGroup.alpha = 0f;
            isPromptVisible = false;
        }
        hasBeenUsed = false;
    }

    private void ApplyInitialVisual()
    {
        if (string.IsNullOrEmpty(syncFlagName))
            return;

        bool flagValue = GameState.GetFlag(syncFlagName);

        // Меняем спрайт
        SpriteRenderer sr = (targetObject != null ? targetObject.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>());
        if (sr != null)
        {
            if (flagValue && onTrueSprite != null)
                sr.sprite = onTrueSprite;
            else if (!flagValue && onFalseSprite != null)
                sr.sprite = onFalseSprite;
        }

        // Меняем активность
        if (modifyActive)
        {
            GameObject obj = targetObject != null ? targetObject : gameObject;
            if (obj != null)
                obj.SetActive(flagValue ? activeWhenTrue : !activeWhenTrue);
        }
    }

    // Обновить видимость подсказки в зависимости от доступности диалога
    private void UpdatePromptVisibility()
    {
        bool available = dialogue != null && !hasBeenUsed && !dialogueTriggeredThisSession;

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