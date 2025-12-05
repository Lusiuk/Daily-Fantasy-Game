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
    private PlayerMovement playerMovement;
    private bool wasPlayerMovementEnabled = true;

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

            Debug.Log("DialogueSystem: Input System reinitialized");
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

        if (disablePlayerMovement)
        {
            DisablePlayerMovement();
        }

        currentDialogue = dialogue;
        isDialogueActive = true;
        inputEnabled = false;

        dialoguePanel.SetActive(true);

        if (currentTypewriter != null)
            StopCoroutine(currentTypewriter);

        StartCoroutine(ShowDialogueSequence());

        OnDialogueStart?.Invoke();
    }

    // Остановка персонажа
    private void DisablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("DialogueSystem: Player not found with tag 'Player'");
            return;
        }

        playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("DialogueSystem: PlayerMovement component not found on player");
            return;
        }

        wasPlayerMovementEnabled = playerMovement.enabled;
        playerMovement.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("Player movement disabled");
    }

    // Управляет последовательностью показа диалога
    private IEnumerator ShowDialogueSequence()
    {
        // Устанавливаем адаптивный размер панели
        if (dialoguePanel != null)
        {
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            if (panelRect != null && useScreenPercentage)
            {
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;

                float width = screenWidth * widthPercentage;
                float height = screenHeight * heightPercentage;

                panelRect.sizeDelta = new Vector2(width, height);

                panelRect.anchoredPosition = Vector2.zero;

                panelRect.anchorMin = new Vector2(0.5f, 0.1f);
                panelRect.anchorMax = new Vector2(0.5f, 0.1f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        // Настраиваем текстовое поле
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

        StartTypewriterMusic();

        currentTypewriter = StartCoroutine(TypewriterEffect(currentDialogue.text));
        yield return currentTypewriter;

        StopTypewriterMusic();

        inputEnabled = true;
    }

    // Запускает музыку печати текста
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
            Debug.Log($"DialogueSystem: Started playing typewriter music: {currentDialogue.typewriterMusic.name}");
        }
    }

    // Останавливает музыку печати текста
    private void StopTypewriterMusic()
    {
        if (typewriterAudioSource != null && typewriterAudioSource.isPlaying)
        {
            typewriterAudioSource.Stop();
            Debug.Log("DialogueSystem: Stopped typewriter music");
        }
    }

    // Скрывает диалоговое окно
    public void HideDialogue()
    {
        if (!isActive || !isDialogueActive || !inputEnabled) return;

        inputEnabled = false;

        StopTypewriterMusic();

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

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (currentDialogue != null)
        {
            if (currentDialogue.teleportAfterDialogue)
            {
                Debug.Log($"DialogueSystem: Teleport requested. Marker: {currentDialogue.teleportMarkerName}, Position: {currentDialogue.teleportPosition}");

                if (currentDialogue.triggerSceneTransition)
                {
                    GameState.ShouldTeleport = true;
                    GameState.TeleportPosition = currentDialogue.teleportPosition;
                    GameState.TeleportMarkerName = currentDialogue.teleportMarkerName;

                    Debug.Log($"DialogueSystem: Will teleport after scene transition to {currentDialogue.targetSceneName}");

                    if (TransitionManager.Instance != null)
                    {
                        TransitionManager.Instance.TransitionToScene(currentDialogue.targetSceneName);
                    }
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
                {
                    TransitionManager.Instance.TransitionToScene(currentDialogue.targetSceneName);
                }
            }
        }

        if (!currentDialogue.triggerSceneTransition && disablePlayerMovement)
        {
            EnablePlayerMovement();
        }

        isDialogueActive = false;
        currentDialogue = null;
        inputEnabled = true;

        OnDialogueEnd?.Invoke();
    }

    // Метод телепортации игрока
    private void TeleportPlayer(Vector2 position, string markerName = "")
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("DialogueSystem: Cannot teleport - player not found");
            return;
        }

        Vector2 targetPosition = position;

        if (!string.IsNullOrEmpty(markerName))
        {
            GameObject marker = GameObject.Find(markerName);
            if (marker != null)
            {
                targetPosition = marker.transform.position;
                Debug.Log($"DialogueSystem: Found teleport marker '{markerName}' at position {targetPosition}");
            }
            else
            {
                Debug.LogWarning($"DialogueSystem: Teleport marker '{markerName}' not found, using default position");
            }
        }

        player.transform.position = targetPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log($"DialogueSystem: Player teleported to {targetPosition}");
    }

    // Пусть идёт
    private void EnablePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = wasPlayerMovementEnabled;

            Debug.Log("Player movement enabled: " + playerMovement.enabled);
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.enabled = true;
                    Debug.Log("Player movement re-enabled");
                }
            }
        }
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