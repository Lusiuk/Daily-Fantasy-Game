using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float typewriterSpeed = 0.05f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference interactAction;

    // События диалога
    public System.Action OnDialogueStart;
    public System.Action OnDialogueEnd;

    // Приватные переменные
    private CanvasGroup canvasGroup;
    private Coroutine currentTypewriter;
    private bool isDialogueActive = false;
    private Dialogue currentDialogue;
    private bool inputEnabled = true;
    private bool isActive = true;

    public static DialogueSystem Instance { get; private set; }

    // Вызывается при создании объекта
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeComponents();
    }

    // Переинициализируем Input System при смене сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"DialogueSystem: Scene loaded - {scene.name}");

        // Переинициализируем Input System
        ReinitializeInputSystem();

        // Находим панель диалога в новой сцене
        FindDialoguePanelInScene();

        // Инициализируем компоненты заново
        InitializeComponents();
    }

    // Обновление подписки на событие
    private void ReinitializeInputSystem()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;

            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;

            Debug.Log("DialogueSystem: Input System переинициализирован");
        }
    }

    // Ищем панель диалога в новой сцене
    private void FindDialoguePanelInScene()
    {
        GameObject panel = GameObject.Find("DialoguePanel");

        if (panel != null)
        {
            dialoguePanel = panel;
            dialogueText = panel.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log("DialogueSystem: Dialogue panel found in new scene");
        }
        else
        {
            Debug.LogWarning("DialogueSystem: Dialogue panel not found in the new scene!");
        }
    }

    // Настраивает систему ввода при активации объекта
    void OnEnable()
    {
        isActive = true;

        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    // Отключает систему ввода при деактивации объекта
    void OnDisable()
    {
        isActive = false;

        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
    }

    // Инициализирует компоненты системы
    void InitializeComponents()
    {
        if (dialoguePanel == null)
        {
            Debug.LogError("DialogueSystem: Dialogue panel is null!");
            return;
        }

        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>() ?? dialoguePanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);
    }

    // Обрабатывает нажатие клавиши взаимодействия
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!isActive || !inputEnabled || !isDialogueActive) return;

        HideDialogue();
    }

    // Показывает диалоговое окно
    public void ShowDialogue(Dialogue dialogue)
    {
        if (!isActive) return;

        if (dialoguePanel == null)
        {
            Debug.LogError("DialogueSystem: Cannot show dialogue - dialoguePanel is null!");
            FindDialoguePanelInScene();
            if (dialoguePanel == null) return;
        }

        if (dialogue == null || !dialogue.canInteract || isDialogueActive) return;

        currentDialogue = dialogue;
        isDialogueActive = true;
        inputEnabled = false;

        dialoguePanel.SetActive(true);

        if (currentTypewriter != null)
            StopCoroutine(currentTypewriter);

        StartCoroutine(ShowDialogueSequence());

        OnDialogueStart?.Invoke();
    }

    // Управляет последовательностью показа диалога
    private IEnumerator ShowDialogueSequence()
    {
        yield return StartCoroutine(FadeDialogue(0f, 1f, fadeDuration));

        currentTypewriter = StartCoroutine(TypewriterEffect(currentDialogue.text));
        yield return currentTypewriter;

        inputEnabled = true;
    }

    // Скрывает диалоговое окно
    public void HideDialogue()
    {
        if (!isActive || !isDialogueActive || !inputEnabled) return;

        inputEnabled = false;

        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
            currentTypewriter = null;
        }

        StartCoroutine(HideDialogueSequence());
    }

    // Управляет последовательностью скрытия диалога
    private IEnumerator HideDialogueSequence()
    {
        yield return StartCoroutine(FadeDialogue(1f, 0f, fadeDuration, true));

        isDialogueActive = false;

        if (currentDialogue != null && currentDialogue.triggerSceneTransition)
        {
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.TransitionToScene(currentDialogue.targetSceneName);
            }
        }

        currentDialogue = null;
        inputEnabled = true;

        OnDialogueEnd?.Invoke();
    }

    // Создаёт эффект печатающегося текста
    private IEnumerator TypewriterEffect(string text)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";

        foreach (char character in text)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    // Управляет плавным изменением прозрачности
    private IEnumerator FadeDialogue(float from, float to, float duration, bool disableAfter = false)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;

        if (disableAfter && dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // Проверяет активен ли диалог
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}