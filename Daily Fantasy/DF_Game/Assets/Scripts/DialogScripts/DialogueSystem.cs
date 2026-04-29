using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Dialogue;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float typewriterSpeed = 0.03f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource typewriterAudioSource;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Adaptive Panel Size")]
    [SerializeField] private bool useScreenPercentage = true;
    [SerializeField][Range(0.1f, 1f)] private float widthPercentage = 0.8f;
    [SerializeField][Range(0.1f, 0.5f)] private float heightPercentage = 0.2f;

    [Header("Player Control")]
    [SerializeField] private bool disablePlayerMovement = true;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportDelay = 0.1f;

    [Header("Question UI")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // События
    public System.Action OnDialogueStart;
    public System.Action OnDialogueEnd;

    // Приватные переменные
    private CanvasGroup canvasGroup;
    private Coroutine currentTypewriter;
    private bool isDialogueActive = false;
    private Dialogue currentDialogue;
    private bool inputEnabled = true;
    private bool isActive = true;
    private PlayerMovement playerMovement;
    private bool wasPlayerMovementEnabled = true;
    private int currentLineIndex = 0;
    private bool isWaitingForNext = false;
    private PlayerPlatformingMovement playerPlatformingMovement;
    private bool isQuestionMode = false;

    public static DialogueSystem Instance { get; private set; }

    // Инициализация синглтона
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

    // После загрузки сцены переподключаем всё
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"DialogueSystem: Scene loaded - {scene.name}");
        ReinitializeInputSystem();
        FindDialoguePanelInScene();
        InitializeComponents();
    }

    // Переподключаем InputSystem
    private void ReinitializeInputSystem()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
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
        }
        if (yesButton == null)
            yesButton = dialoguePanel?.transform.Find("YesButton")?.GetComponent<Button>();
        if (noButton == null)
            noButton = dialoguePanel?.transform.Find("NoButton")?.GetComponent<Button>();
    }

    // Включение/отключение ввода
    void OnEnable()
    {
        isActive = true;
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    void OnDisable()
    {
        isActive = false;
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
    }

    // Инициализация компонентов панели
    void InitializeComponents()
    {
        if (dialoguePanel == null) return;
        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>() ?? dialoguePanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);
    }

    // Обработка нажатия кнопки взаимодействия
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!isActive || !isDialogueActive) return;
        if (isQuestionMode) return;

        // Если ждём следующую реплику – переходим к ней
        if (isWaitingForNext)
        {
            isWaitingForNext = false;
            return;
        }

        // Иначе закрываем диалог
        if (inputEnabled)
            HideDialogue();
    }

    // Показ диалога
    public void ShowDialogue(Dialogue dialogue)
    {
        if (!isActive) return;
        if (dialoguePanel == null)
        {
            FindDialoguePanelInScene();
            if (dialoguePanel == null) return;
        }
        if (dialogue == null || !dialogue.canInteract || isDialogueActive) return;

        if (disablePlayerMovement) DisablePlayerMovement();

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        inputEnabled = false;
        isWaitingForNext = false;
        dialoguePanel.SetActive(true);

        if (currentTypewriter != null) StopCoroutine(currentTypewriter);
        StartCoroutine(ShowDialogueSequence());

        OnDialogueStart?.Invoke();
    }

    // Отключение движения игрока
    private void DisablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Отключаем движение
        playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            wasPlayerMovementEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

        // Отключаем платформенное движение
        playerPlatformingMovement = player.GetComponent<PlayerPlatformingMovement>();
        if (playerPlatformingMovement != null)
        {
            playerPlatformingMovement.enabled = false;
        }

        // Отключаем аниматор
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Останавливаем физическое движение
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("Player movement and animation disabled");
    }

    // Включение движения игрока
    private void EnablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Включаем движение
        if (playerMovement != null)
        {
            playerMovement.enabled = wasPlayerMovementEnabled;
        }
        else
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = true;
        }

        // Включаем платформенное движение
        if (playerPlatformingMovement != null)
        {
            playerPlatformingMovement.enabled = true;
        }

        // Включаем аниматор
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
        }

        Debug.Log("Player movement and animation enabled");
    }

    public void DisablePlayerMovementPublic()
    {
        DisablePlayerMovement();
    }

    public void EnablePlayerMovementPublic()
    {
        EnablePlayerMovement();
    }

    // Основная корутина показа диалога (поддерживает последовательные реплики)
    private IEnumerator ShowDialogueSequence()
    {
        // Адаптация размера панели
        if (dialoguePanel != null)
        {
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            if (panelRect != null && useScreenPercentage)
            {
                float width = Screen.width * widthPercentage;
                float height = Screen.height * heightPercentage;
                panelRect.sizeDelta = new Vector2(width, height);
                panelRect.anchoredPosition = Vector2.zero;
                panelRect.anchorMin = new Vector2(0.5f, 0.1f);
                panelRect.anchorMax = new Vector2(0.5f, 0.1f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        // Настройка текстового поля
        if (dialogueText != null)
        {
            RectTransform textRect = dialogueText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.pivot = new Vector2(0.5f, 0.5f);
                float padding = 20f;
                textRect.offsetMin = new Vector2(padding, padding);
                textRect.offsetMax = new Vector2(-padding, -padding);
                dialogueText.horizontalAlignment = HorizontalAlignmentOptions.Center;
                dialogueText.verticalAlignment = VerticalAlignmentOptions.Middle;
            }
        }

        yield return StartCoroutine(FadeDialogue(0f, 1f, fadeDuration));

        // Цикл по репликам
        while (true)
        {
            string displayText = GetCurrentLineText();
            StartTypewriterMusic();
            currentTypewriter = StartCoroutine(TypewriterEffect(displayText));
            yield return currentTypewriter;
            StopTypewriterMusic();

            int totalLines = currentDialogue.lines?.Length ?? 0;
            if (totalLines > 0)
            {
                // Если есть следующая реплика – ждём нажатия
                if (currentLineIndex + 1 >= totalLines)
                    break;
                isWaitingForNext = true;
                yield return new WaitUntil(() => !isWaitingForNext);
                currentLineIndex++;
                continue;
            }
            else
            {
                // Одиночный диалог (старый формат)
                break;
            }
        }

        inputEnabled = true;
    }

    // Получение текущей реплики
    private DialogueLine GetCurrentLine()
    {
        if (currentDialogue.lines != null && currentDialogue.lines.Length > 0)
            return currentDialogue.lines[currentLineIndex];
        else
            return new DialogueLine { text = currentDialogue.text, isPlayerSpeaking = currentDialogue.isPlayerSpeaking };
    }

    // Формирование строки с именем говорящего
    private string GetCurrentLineText()
    {
        var line = GetCurrentLine();
        if (line.isPlayerSpeaking)
            return $"[Женя]: {line.text}";
        else
        {
            string name = string.IsNullOrEmpty(line.npcName) ? currentDialogue.npcName : line.npcName;
            if (string.IsNullOrEmpty(name)) name = "NPC";
            return $"[{name}]: {line.text}";
        }
    }

    // Запуск музыки печати
    private void StartTypewriterMusic()
    {
        if (typewriterAudioSource == null)
        {
            typewriterAudioSource = gameObject.AddComponent<AudioSource>();
            typewriterAudioSource.playOnAwake = false;
        }
        if (currentDialogue != null && currentDialogue.typewriterMusic != null)
        {
            typewriterAudioSource.clip = currentDialogue.typewriterMusic;
            typewriterAudioSource.loop = currentDialogue.loopMusic;
            typewriterAudioSource.Play();
        }
    }

    // Остановка музыки печати
    private void StopTypewriterMusic()
    {
        if (typewriterAudioSource != null && typewriterAudioSource.isPlaying)
            typewriterAudioSource.Stop();
    }

    // Скрытие диалога
    public void HideDialogue()
    {
        if (!isActive || !isDialogueActive || !inputEnabled) return;
        inputEnabled = false;
        isWaitingForNext = false;
        StopTypewriterMusic();
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
            currentTypewriter = null;
        }
        StartCoroutine(HideDialogueSequence());
    }

    // Последовательность скрытия с возможными переходами и телепортацией
    private IEnumerator HideDialogueSequence()
    {
        yield return StartCoroutine(FadeDialogue(1f, 0f, fadeDuration, true));

        if (dialogueText != null) dialogueText.text = "";

        if (currentDialogue != null)
        {
            if (currentDialogue.teleportAfterDialogue)
            {
                if (currentDialogue.triggerSceneTransition)
                {
                    GameState.ShouldTeleport = true;
                    GameState.TeleportPosition = currentDialogue.teleportPosition;
                    GameState.TeleportMarkerName = currentDialogue.teleportMarkerName;
                    if (TransitionManager.Instance != null)
                        TransitionManager.Instance.TransitionToScene(currentDialogue.targetSceneName);
                }
                else
                {
                    yield return new WaitForSeconds(teleportDelay);
                    TeleportPlayer(currentDialogue.teleportPosition, currentDialogue.teleportMarkerName);
                }
            }
            else if (currentDialogue.triggerSceneTransition)
            {
                if (TransitionManager.Instance != null)
                    TransitionManager.Instance.TransitionToScene(currentDialogue.targetSceneName);
            }
        }

        if (!currentDialogue.triggerSceneTransition && disablePlayerMovement)
            EnablePlayerMovement();

        isDialogueActive = false;
        currentDialogue = null;
        inputEnabled = true;
        OnDialogueEnd?.Invoke();
    }

    // Телепортация игрока
    private void TeleportPlayer(Vector2 position, string markerName = "")
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector2 targetPosition = position;
        if (!string.IsNullOrEmpty(markerName))
        {
            GameObject marker = GameObject.Find(markerName);
            if (marker != null)
                targetPosition = marker.transform.position;
        }
        player.transform.position = targetPosition;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // Эффект печатной машинки
    private IEnumerator TypewriterEffect(string textToPrint)
    {
        if (dialogueText == null) yield break;
        dialogueText.text = "";
        foreach (char character in textToPrint)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    // Плавное появление/исчезновение панели
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
        if (disableAfter && dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    // Проверка активен ли диалог
    public bool IsDialogueActive() => isDialogueActive;

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }



    public void AskQuestion(string question, Action<bool> callback)
    {
        if (!isActive || isDialogueActive || isQuestionMode) return;
        StartCoroutine(AskQuestionRoutine(question, callback));
    }

    private IEnumerator AskQuestionRoutine(string question, Action<bool> callback)
    {
        isQuestionMode = true;
        if (dialoguePanel == null) yield break;

        // Показываем панель
        dialoguePanel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Печатаем вопрос
        if (dialogueText != null)
        {
            dialogueText.text = "";
            foreach (char c in question)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        // Показываем кнопки
        if (yesButton != null) yesButton.gameObject.SetActive(true);
        if (noButton != null) noButton.gameObject.SetActive(true);

        bool? answer = null;
        UnityEngine.Events.UnityAction yesAction = null;
        UnityEngine.Events.UnityAction noAction = null;

        yesAction = () => {
            answer = true;
            yesButton.onClick.RemoveListener(yesAction);
            noButton.onClick.RemoveListener(noAction);
        };
        noAction = () => {
            answer = false;
            yesButton.onClick.RemoveListener(yesAction);
            noButton.onClick.RemoveListener(noAction);
        };

        yesButton.onClick.AddListener(yesAction);
        noButton.onClick.AddListener(noAction);

        yield return new WaitUntil(() => answer.HasValue);

        // Скрываем кнопки и панель
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";

        isQuestionMode = false;
        callback?.Invoke(answer.Value);
    }
}