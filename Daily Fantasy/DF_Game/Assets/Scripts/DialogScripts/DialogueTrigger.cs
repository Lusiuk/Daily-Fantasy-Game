using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;
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

    [System.Serializable]
    public class DialogueReplacement
    {
        public string[] conditions;
        public Dialogue dialogue;
    }

    [Header("Dialogue Replacement")]
    [SerializeField] private DialogueReplacement[] replacements; // список замен

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

        UpdateMinigameBlock();
        CheckAndApplyReplacement(); // сначала замены

        // Проверка, не является ли текущий диалог
        if (dialogue != null && dialogue.oneTimeUse && !string.IsNullOrEmpty(dialogue.dialogueId) && GameState.IsDialogueUsed(dialogue.dialogueId))
        {
            DisableTrigger();
            return;
        }

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
        // Если изменился флаг, относящийся к этой мини-игре, обновляем блокировку
        UpdateMinigameBlock();

        CheckAndApplyReplacement();

        if (playerInRange)
        {
            UpdatePromptVisibility();
        }
    }

    private void UpdateMinigameBlock()
    {
        if (!isMinigameTrigger) return;

        bool isCompleted = false;
        switch (minigameName)
        {
            case "IDEMinigame":
                isCompleted = GameState.IsIDEMinigameCompleted;
                break;
            case "Platforming":
                isCompleted = GameState.IsPlatformingCompleted;
                break;
            case "RhythmGame1":
                isCompleted = GameState.IsRhythmGame1Completed;
                break;
            case "RhythmGame2":
                isCompleted = GameState.IsRhythmGame2Completed;
                break;
            default:
                return;
        }

        if (isCompleted)
        {
            if (interactionPrompt != null)
            {
                promptCanvasGroup.alpha = 0f;
                interactionPrompt.SetActive(false);
                isPromptVisible = false;
            }
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
        }
        else
        {
            // Если ещё не пройдена, включаем коллайдер и подсказку
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                promptCanvasGroup.alpha = 0f;
                isPromptVisible = false;
            }
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

        DialogueSystem.Instance.ShowDialogue(dialogue);

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

        // Начинаем проверку со следующей после уже применённой замены
        int startIndex = appliedReplacementIndex + 1;
        for (int i = startIndex; i < replacements.Length; i++)
        {
            var repl = replacements[i];
            if (repl.dialogue == null) continue;
            if (repl.conditions == null || repl.conditions.Length == 0) continue;

            if (GameState.AreFlagsSatisfied(repl.conditions))
            {
                // Применяем замену
                dialogue = repl.dialogue;
                appliedReplacementIndex = i;
                Debug.Log($"{gameObject.name}: диалог заменён на {dialogue.name} (условия #{i})");

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
                return;
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