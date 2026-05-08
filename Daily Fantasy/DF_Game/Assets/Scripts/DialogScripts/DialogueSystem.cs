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
    [SerializeField] private AudioClip questionMusic;

    // События
    public System.Action OnDialogueStart;
    public System.Action OnDialogueEnd;

    //Какой объект открыл диалог
    public GameObject CurrentContextObject => currentContextObject;

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
    private GameObject currentContextObject;

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

        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
            currentTypewriter = null;
        }

        StopTypewriterMusic();

        isDialogueActive = false;
        isQuestionMode = false;
        currentDialogue = null;
        currentContextObject = null;
        inputEnabled = true;
        isWaitingForNext = false;

        // Сброс ссылок на компоненты игрока (чтобы не держать уничтоженные объекты)
        playerMovement = null;
        playerPlatformingMovement = null;

        ReinitializeInputSystem();
        FindDialoguePanelInScene();
        InitializeComponents();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
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
    public void ShowDialogue(Dialogue dialogue, GameObject contextObject = null)
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
        currentContextObject = contextObject;
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
                if (currentDialogue.centerDialoguePanel)
                {
                    panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                }
                else
                {
                    panelRect.anchorMin = new Vector2(0.5f, 0.1f);
                    panelRect.anchorMax = new Vector2(0.5f, 0.1f);
                }
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

    public void StopMusic()
    {
        if (typewriterAudioSource != null && typewriterAudioSource.isPlaying)
            typewriterAudioSource.Stop();

        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
            currentTypewriter = null;
        }
        isDialogueActive = false;
        isQuestionMode = false;
        currentDialogue = null;
        currentContextObject = null;
        inputEnabled = true;
        isWaitingForNext = false;
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

        QuestDialogue questDialogue = currentDialogue as QuestDialogue;

        // Обработка обычных переходов (телепорт, смена сцены) – без изменений…
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
                    goto EndDialogue;
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
                goto EndDialogue;
            }
        }

        // Квестовая часть
        if (questDialogue != null && !string.IsNullOrEmpty(questDialogue.questionText))
        {
            bool? answer = null;
            AskQuestion(questDialogue.questionText, (result) => { answer = result; });
            yield return new WaitUntil(() => answer.HasValue);

            bool yes = answer.Value;
            Outcome chosenOutcome = yes ? questDialogue.positiveOutcome : questDialogue.negativeOutcome;
            bool playBlink = yes ? questDialogue.playEyeBlinkOnYes : questDialogue.playEyeBlinkOnNo;

            FinalEndingDialogue finalEnding = questDialogue as FinalEndingDialogue;

            if (finalEnding != null)
            {
                if (yes)
                {
                    // Применяем исход (флаги и т.д.)
                    if (chosenOutcome != null)
                        ExecuteOutcome(chosenOutcome);

                    // Скрываем игрока, если нужно
                    if (finalEnding.hidePlayer)
                    {
                        GameObject player = GameObject.FindGameObjectWithTag("Player");
                        if (player != null) player.SetActive(false);
                    }

                    // Меняем спрайт кровати
                    if (currentContextObject != null && finalEnding.bedFinalSprite != null)
                    {
                        SpriteRenderer sr = currentContextObject.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = finalEnding.bedFinalSprite;
                            currentContextObject.transform.localScale = finalEnding.bedFinalScale;
                        }
                    }

                    // Кастомное моргание
                    if (finalEnding.blinkSteps != null && finalEnding.blinkSteps.Length > 0 && TransitionManager.Instance != null)
                    {
                        yield return TransitionManager.Instance.PlayCustomBlink(finalEnding.blinkSteps, finalEnding.blinkStepDuration);
                    }

                    StopTypewriterMusic();
                    if (currentTypewriter != null)
                    {
                        StopCoroutine(currentTypewriter);
                        currentTypewriter = null;
                    }
                    isDialogueActive = false;
                    isQuestionMode = false;
                    currentDialogue = null;
                    currentContextObject = null;
                    inputEnabled = true;
                    isWaitingForNext = false;

                    // Выбираем финальный диалог
                    Dialogue chosenFinalDialogue = null;

                    if (finalEnding.finalDialogueOptions != null && finalEnding.finalDialogueOptions.Length > 0)
                    {
                        foreach (var option in finalEnding.finalDialogueOptions)
                        {
                            if (GameState.AreFlagsSatisfied(option.conditions))
                            {
                                chosenFinalDialogue = option.dialogue;
                                break;
                            }
                        }
                    }

                    if (chosenFinalDialogue == null)
                        chosenFinalDialogue = finalEnding.finalDialogue;

                    // Запускаем финальный диалог
                    if (chosenFinalDialogue != null)
                    {
                        // Поднимаем диалоговый канвас над чёрными полосами
                        DialogueSystem.Instance.SetCanvasOrder(999);

                        DialogueSystem.Instance.ShowDialogue(chosenFinalDialogue, null);
                        yield return new WaitUntil(() => !DialogueSystem.Instance.IsDialogueActive());

                        // Возвращаем исходный порядок
                        DialogueSystem.Instance.SetCanvasOrder(-1);
                    }
                }
                else
                {
                    // Ответ «Нет» – просто возвращаем управление игроку
                    if (disablePlayerMovement)
                        EnablePlayerMovement();
                }
            }
            else
            {
                // Стандартная обработка квеста (с эффектом моргания)
                try
                {
                    if (playBlink && TransitionManager.Instance != null)
                        yield return TransitionManager.Instance.CloseBars();

                    if (chosenOutcome != null)
                        ExecuteOutcome(chosenOutcome);

                    if (playBlink && TransitionManager.Instance != null)
                        yield return TransitionManager.Instance.OpenBars();
                }
                finally
                {
                    if (disablePlayerMovement)
                        EnablePlayerMovement();
                }
            }
        }

        // Включаем движение только для не-квестовых диалогов (и не финала)
        if (!questDialogue && !currentDialogue.triggerSceneTransition && disablePlayerMovement)
            EnablePlayerMovement();

        // Полный сброс состояний
        StopTypewriterMusic();
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
            currentTypewriter = null;
        }
        isDialogueActive = false;
        isQuestionMode = false;
        currentDialogue = null;
        currentContextObject = null;
        inputEnabled = true;
        isWaitingForNext = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        EndDialogue:
        isDialogueActive = false;
        currentDialogue = null;
        currentContextObject = null;
        inputEnabled = true;
        OnDialogueEnd?.Invoke();
    }

    private void ExecuteOutcome(Outcome outcome)
    {
        if (outcome == null) return;

        // Флаги
        foreach (var change in outcome.flagChanges)
            GameState.SetFlag(change.flagName, change.value);

        // Спрайт и активность через контекст
        if (currentContextObject != null)
        {
            if (outcome.newSprite != null)
            {
                SpriteRenderer sr = currentContextObject.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = outcome.newSprite;
            }

            // Управление целевым объектом
            GameObject target = outcome.targetObject != null ? outcome.targetObject : currentContextObject;
            if (target != null)
                target.SetActive(outcome.setActive);
        }

        outcome.onComplete.Invoke();
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
        if (currentTypewriter != null)
            StopCoroutine(currentTypewriter);
        isDialogueActive = false;
    }



    public void AskQuestion(string question, Action<bool> callback)
    {
        if (!isActive || isQuestionMode) return;
        StartCoroutine(AskQuestionRoutine(question, callback));
    }

    private IEnumerator AskQuestionRoutine(string question, Action<bool> callback)
    {
        isQuestionMode = true;
        if (dialoguePanel == null) yield break;

        // Показываем панель
        dialoguePanel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Запускаем музыку вопроса (только на время печати)
        if (questionMusic != null)
        {
            if (typewriterAudioSource == null)
            {
                typewriterAudioSource = gameObject.AddComponent<AudioSource>();
                typewriterAudioSource.playOnAwake = false;
            }
            typewriterAudioSource.clip = questionMusic;
            typewriterAudioSource.loop = false;
            typewriterAudioSource.Play();
        }

        // Добавляем подсказку Y/N
        string fullText = question + " (Y/N)";

        // Печатаем вопрос
        if (dialogueText != null)
        {
            dialogueText.text = "";
            foreach (char c in fullText)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        // Остановка музыки сразу после завершения печати
        StopQuestionMusic();

        // Ждём нажатия Y или N
        bool? answer = null;
        Keyboard keyboard = Keyboard.current;
        while (!answer.HasValue)
        {
            if (keyboard != null)
            {
                if (keyboard.yKey.wasPressedThisFrame)
                    answer = true;
                else if (keyboard.nKey.wasPressedThisFrame)
                    answer = false;
            }
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Y)) answer = true;
            else if (Input.GetKeyDown(KeyCode.N)) answer = false;
#endif
            yield return null;
        }

        // Скрываем панель
        dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";

        isQuestionMode = false;
        callback?.Invoke(answer.Value);
    }

    private void StopQuestionMusic()
    {
        if (typewriterAudioSource != null && typewriterAudioSource.isPlaying)
            typewriterAudioSource.Stop();
    }

    private int originalCanvasOrder = 0;
    private Canvas dialogueCanvas;

    public void SetCanvasOrder(int order)
    {
        if (dialogueCanvas == null)
            dialogueCanvas = dialoguePanel?.GetComponentInParent<Canvas>();
        if (dialogueCanvas != null)
        {
            if (order == -1) // специальное значение для восстановления
                dialogueCanvas.sortingOrder = originalCanvasOrder;
            else
            {
                if (originalCanvasOrder == 0) originalCanvasOrder = dialogueCanvas.sortingOrder;
                dialogueCanvas.sortingOrder = order;
            }
        }
    }
}