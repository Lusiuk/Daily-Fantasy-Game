using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[System.Serializable]
public class Condition
{
    public string flagName;
    public bool _not = false;
}

[System.Serializable]
public class Outcome
{
    [System.Serializable]
    public struct FlagChange
    {
        public string flagName;
        public bool value;
    }

    public FlagChange[] flagChanges;    // установить указанные флаги
    public Sprite newSprite;            // новый спрайт для SpriteRenderer (если есть)
    public GameObject targetObject;     // объект, который нужно активировать/деактивировать
    public bool setActive = true;       // что сделать с targetObject (true - включить)
    public UnityEvent onComplete;       // дополнительные действия
}

[System.Serializable]
public class InteractiveAction
{
    public string actionName;               // для удобства дизайнера
    [TextArea(1, 3)]
    public string questionText;             // текст, который будет показан перед выбором (или сразу)
    public bool requiresChoice = false;     // нужен выбор Да/Нет
    public string[] conditions;             // условия доступности (стандартный формат)
    public Outcome positiveOutcome;         // исход при "Да" или автоматический
    public Outcome negativeOutcome;         // исход при "Нет" (только если requiresChoice)
}

[System.Serializable]
public class VisualState
{
    public string stateName;
    public string[] conditions;             // при каких условиях это состояние активно
    public Sprite sprite;                   // спрайт для SpriteRenderer
    public bool setActive = true;           // активность объекта (если targetObject не указан, то самого себя)
    public GameObject targetObject;         // если нужно управлять другим объектом
}

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractiveAction[] actions;

    [Header("Visual States")]
    public VisualState[] visualStates;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Prompt (optional)")]
    [SerializeField] private GameObject interactionPrompt;   // если назначен, будет показываться
    [SerializeField] private float promptOffset = 4f;
    [SerializeField] private float uiAdditionalOffset = 90f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float fadePromptSpeed = 5f;

    // Ссылка на DialogueTrigger для запуска диалога, когда действий нет
    private DialogueTrigger dialogueTrigger;

    // Внутренние
    private bool playerInRange = false;
    private Transform playerTransform;
    private Camera mainCamera;
    private RectTransform promptRectTransform;
    private CanvasGroup promptCanvasGroup;
    private bool isPromptVisible = false;
    private Vector2 targetScreenPosition;
    private Vector2 currentVelocity = Vector2.zero;
    private SpriteRenderer spriteRenderer;

    private bool isPerformingAction = false; // блокировка повторных нажатий

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Находим DialogueTrigger на том же объекте, если есть, и отключаем его ввод
        dialogueTrigger = GetComponent<DialogueTrigger>();
        if (dialogueTrigger != null)
        {
            dialogueTrigger.disableInputHandling = true;
        }

        // Инициализация подсказки
        if (interactionPrompt != null)
        {
            promptRectTransform = interactionPrompt.GetComponent<RectTransform>();
            promptCanvasGroup = interactionPrompt.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = interactionPrompt.AddComponent<CanvasGroup>();
                promptCanvasGroup.alpha = 0f;
                promptCanvasGroup.interactable = false;
                promptCanvasGroup.blocksRaycasts = false;
            }
            interactionPrompt.SetActive(true);
            isPromptVisible = false;
        }

        // Подписка на ввод
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }

        // Подписка на изменения флагов для обновления визуала
        GameState.OnFlagChanged += OnFlagChanged;
        UpdateVisual();
    }

    void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
        GameState.OnFlagChanged += OnFlagChanged;
        UpdateVisual();
    }

    void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
        GameState.OnFlagChanged -= OnFlagChanged;
    }

    private void OnFlagChanged(string flagName)
    {
        UpdateVisual();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
            HidePrompt();
        }
    }

    void Update()
    {
        if (!playerInRange || interactionPrompt == null || !isPromptVisible)
            return;

        // Плавное позиционирование подсказки над игроком
        if (playerTransform != null && mainCamera != null)
        {
            Vector3 worldPos = playerTransform.position + Vector3.up * promptOffset;
            Vector2 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            screenPos.y += uiAdditionalOffset;

            if (Vector2.Distance(targetScreenPosition, screenPos) > 2f)
                targetScreenPosition = screenPos;

            if (promptRectTransform != null)
            {
                promptRectTransform.position = Vector2.SmoothDamp(
                    promptRectTransform.position, targetScreenPosition,
                    ref currentVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            }
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!playerInRange || isPerformingAction || DialogueSystem.Instance == null)
            return;

        if (DialogueSystem.Instance.IsDialogueActive())
            return;

        // Ищем первое доступное действие
        InteractiveAction availableAction = null;
        foreach (var act in actions)
        {
            if (GameState.AreFlagsSatisfied(act.conditions))
            {
                availableAction = act;
                break;
            }
        }

        if (availableAction != null)
        {
            isPerformingAction = true;
            StartCoroutine(PerformAction(availableAction));
        }
        else
        {
            // Действий нет – запускаем диалог через DialogueTrigger
            if (dialogueTrigger != null)
            {
                dialogueTrigger.TriggerDialogue();
            }
        }
    }

    private IEnumerator PerformAction(InteractiveAction action)
    {
        // Блокируем управление игроком (используем методы DialogueSystem)
        DialogueSystem.Instance.DisablePlayerMovementPublic();

        if (action.requiresChoice)
        {
            // Показываем вопрос и ждём ответ
            bool? answer = null;
            DialogueSystem.Instance.AskQuestion(action.questionText, (result) => {
                answer = result;
            });

            yield return new WaitUntil(() => answer.HasValue);

            if (answer.Value)
                ExecuteOutcome(action.positiveOutcome);
            else if (action.negativeOutcome != null)
                ExecuteOutcome(action.negativeOutcome);
        }
        else
        {
            // Без вопроса – просто выполняем положительный исход
            ExecuteOutcome(action.positiveOutcome);
            // Небольшая пауза, чтобы игрок заметил результат
            yield return new WaitForSeconds(0.2f);
        }

        DialogueSystem.Instance.EnablePlayerMovementPublic();
        isPerformingAction = false;
        UpdateVisual();
        HidePrompt();
    }

    private void ExecuteOutcome(Outcome outcome)
    {
        if (outcome == null) return;

        // Изменяем флаги
        foreach (var change in outcome.flagChanges)
        {
            GameState.SetFlag(change.flagName, change.value);
        }

        // Меняем спрайт
        if (outcome.newSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = outcome.newSprite;

        // Включаем/выключаем объект
        if (outcome.targetObject != null)
            outcome.targetObject.SetActive(outcome.setActive);

        // Дополнительные события
        outcome.onComplete.Invoke();
    }

    private void UpdateVisual()
    {
        foreach (var state in visualStates)
        {
            if (GameState.AreFlagsSatisfied(state.conditions))
            {
                if (state.sprite != null && spriteRenderer != null)
                    spriteRenderer.sprite = state.sprite;

                GameObject target = state.targetObject != null ? state.targetObject : gameObject;
                if (target != null)
                    target.SetActive(state.setActive);
                break;
            }
        }
    }

    private void ShowPrompt()
    {
        if (promptCanvasGroup == null || isPromptVisible) return;
        isPromptVisible = true;
        StartCoroutine(FadePrompt(0f, 1f, fadePromptSpeed));
    }

    private void HidePrompt()
    {
        if (promptCanvasGroup == null || !isPromptVisible) return;
        isPromptVisible = false;
        StartCoroutine(FadePrompt(promptCanvasGroup.alpha, 0f, fadePromptSpeed));
    }

    private IEnumerator FadePrompt(float from, float to, float speed)
    {
        float duration = Mathf.Abs(to - from) / speed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        promptCanvasGroup.alpha = to;
    }

    private void OnDestroy()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPerformed;
        GameState.OnFlagChanged -= OnFlagChanged;
    }
}