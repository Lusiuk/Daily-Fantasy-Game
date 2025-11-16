using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

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

    public static DialogueSystem Instance { get; private set; }

    // Вызывается при создании объекта
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeComponents();
    }

    // Настраивает систему ввода при активации объекта
    void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    // Отключает систему ввода при деактивации объекта
    void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
    }

    // Инициализирует компоненты системы
    void InitializeComponents()
    {
        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>() ?? dialoguePanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);
    }

    // Обрабатывает нажатие клавиши взаимодействия
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!inputEnabled || !isDialogueActive) return;

        HideDialogue();
    }

    // Показывает диалоговое окно
    public void ShowDialogue(Dialogue dialogue)
    {
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
        if (!isDialogueActive || !inputEnabled) return;

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
        currentDialogue = null;

        inputEnabled = true;

        OnDialogueEnd?.Invoke();
    }

    // Создаёт эффект печатающегося текста
    private IEnumerator TypewriterEffect(string text)
    {
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
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;

        if (disableAfter)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // Проверяет активен ли диалог
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}