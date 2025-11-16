using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button closeButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float typewriterSpeed = 0.05f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.R;

    public System.Action OnDialogueStart;
    public System.Action OnDialogueEnd;

    private CanvasGroup canvasGroup;
    private Coroutine currentTypewriter;
    private bool isDialogueActive = false;
    private Dialogue currentDialogue; 

    public static DialogueSystem Instance { get; private set; }

    // Метод Awake вызывается при создании объекта
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

    // Инициализация всех необходимых компонентов
    void InitializeComponents()
    {
        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HideDialogue);
        }
    }

    // Update вызывается каждый кадр
    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(interactKey))
        {
            HideDialogue();
        }
    }

    /// <summary>
    /// Показывает диалоговое окно с указанным текстом
    /// </summary>
    /// <param name="dialogue">Объект диалога для отображения</param>
    public void ShowDialogue(Dialogue dialogue)
    {
        if (dialogue == null || !dialogue.canInteract || isDialogueActive)
            return;

        currentDialogue = dialogue;
        isDialogueActive = true;

        dialoguePanel.SetActive(true);

        StartCoroutine(FadeDialogue(0f, 1f, fadeDuration));

        if (currentTypewriter != null)
            StopCoroutine(currentTypewriter);

        currentTypewriter = StartCoroutine(TypewriterEffect(dialogue.text));

        OnDialogueStart?.Invoke();
    }

    /// <summary>
    /// Скрывает диалоговое окно
    /// </summary>
    public void HideDialogue()
    {
        if (!isDialogueActive) return;

        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
            currentTypewriter = null;
        }

        StartCoroutine(FadeDialogue(1f, 0f, fadeDuration, true));

        OnDialogueEnd?.Invoke();

        isDialogueActive = false;
        currentDialogue = null;
    }

    /// <summary>
    /// Эффект печатной машинки для текста
    /// </summary>
    /// <param name="text">Текст для отображения</param>
    private IEnumerator TypewriterEffect(string text)
    {
        dialogueText.text = "";

        foreach (char character in text)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        currentTypewriter = null;
    }

    /// <summary>
    /// Анимация плавного появления/исчезновения диалогового окна
    /// </summary>
    /// <param name="from">Начальная прозрачность (0-1)</param>
    /// <param name="to">Конечная прозрачность (0-1)</param>
    /// <param name="duration">Длительность анимации в секундах</param>
    /// <param name="disableAfter">Отключить панель после анимации?</param>
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

    /// <summary>
    /// Проверяет, активно ли сейчас диалоговое окно
    /// </summary>
    /// <returns>true если диалог активен</returns>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}