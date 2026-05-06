using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public static TransitionManager Instance { get; private set; }

    private Image topBlackBar;
    private Image bottomBlackBar;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            GameState.InitialScene = SceneManager.GetActiveScene().name;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        FindBlackBarsInScene();

        if (SceneManager.GetActiveScene().name == GameState.InitialScene && GameState.IsFirstLoad)
        {
            SetBarsHeight(0f);
        }
    }

    // Загрузка сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // После загрузки сцены находим черные полосы заново
        Debug.Log($"TransitionManager: Scene loaded - {scene.name}");
        FindBlackBarsInScene();

        // Если это начальная сцена и первый запуск
        if (scene.name == GameState.InitialScene && GameState.IsFirstLoad)
        {
            SetBarsHeight(0f);
            GameState.IsFirstLoad = false;
            return;
        }

        // Устанавливаем полосы в закрытое состояние
        if (topBlackBar != null && bottomBlackBar != null)
        {
            SetBarsHeight(0.5f);
        }

        TeleportPlayerIfNeeded();

        StartCoroutine(CompleteTransitionAfterDelay());
    }

    // Телепортация игрока после загрузки сцены
    private void TeleportPlayerIfNeeded()
    {
        if (!GameState.ShouldTeleport) return;

        Debug.Log($"TransitionManager: Teleporting player to marker '{GameState.TeleportMarkerName}' at position {GameState.TeleportPosition}");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("TransitionManager: Cannot teleport - player not found");
            return;
        }

        Vector2 targetPosition = GameState.TeleportPosition;

        if (!string.IsNullOrEmpty(GameState.TeleportMarkerName))
        {
            GameObject marker = GameObject.Find(GameState.TeleportMarkerName);
            if (marker != null)
            {
                targetPosition = marker.transform.position;
                Debug.Log($"TransitionManager: Found teleport marker '{GameState.TeleportMarkerName}' at position {targetPosition}");
            }
            else
            {
                Debug.LogWarning($"TransitionManager: Teleport marker '{GameState.TeleportMarkerName}' not found, using default position");
            }
        }

        player.transform.position = targetPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log($"TransitionManager: Player teleported to {targetPosition}");

        GameState.ShouldTeleport = false;
        GameState.TeleportMarkerName = "";
    }

    private IEnumerator CompleteTransitionAfterDelay()
    {
        if (topBlackBar != null && bottomBlackBar != null)
        {
            yield return StartCoroutine(AnimateBars(0.5f, 0f, transitionDuration / 2));
        }

        isTransitioning = false;
        Debug.Log("Transition completed");
    }

    private void FindBlackBarsInScene()
    {
        GameObject transitionCanvas = GameObject.Find("TransitionCanvas");
        if (transitionCanvas != null)
        {
            Transform topBarTransform = transitionCanvas.transform.Find("TopBlackBar");
            Transform bottomBarTransform = transitionCanvas.transform.Find("BottomBlackBar");

            if (topBarTransform != null)
                topBlackBar = topBarTransform.GetComponent<Image>();
            if (bottomBarTransform != null)
                bottomBlackBar = bottomBarTransform.GetComponent<Image>();

            if (topBlackBar != null && bottomBlackBar != null)
            {
                Debug.Log("Black bars found successfully!");
            }
            else
            {
                Debug.LogWarning("Some black bars are missing in the scene.");
            }
        }
        else
        {
            Debug.LogError("TransitionCanvas not found in the scene!");
        }
    }

    // Запускает переход на другую сцену с анимацией
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;

        GameState.PreviousScene = SceneManager.GetActiveScene().name;

        StartCoroutine(TransitionCoroutine(sceneName));
    }

    // Анимация закрытия полос
    public IEnumerator CloseBars(float durationScale = 1f)
    {
        if (topBlackBar == null || bottomBlackBar == null)
        {
            Debug.LogWarning("TransitionManager.CloseBars: чёрные полосы не найдены");
            yield break;
        }
        float duration = (transitionDuration / 2f) * durationScale;
        yield return StartCoroutine(AnimateBars(0f, 0.5f, duration));
    }

    // Анимация открытия полос
    public IEnumerator OpenBars(float durationScale = 1f)
    {
        if (topBlackBar == null || bottomBlackBar == null)
        {
            Debug.LogWarning("TransitionManager.OpenBars: чёрные полосы не найдены");
            yield break;
        }
        float duration = (transitionDuration / 2f) * durationScale;
        yield return StartCoroutine(AnimateBars(0.5f, 0f, duration));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        Debug.Log("Starting transition - closing animation");

        // Шаг 1: Анимация закрытия
        yield return StartCoroutine(AnimateBars(0f, 0.5f, transitionDuration / 2));

        Debug.Log($"Loading scene: {sceneName}");

        // Шаг 2: Загрузка новой сцены
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Дальнейшая анимация выполняется в OnSceneLoaded
    }

    // Анимирует движение черных полос
    private IEnumerator AnimateBars(float fromHeight, float toHeight, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            float curvedProgress = animationCurve.Evaluate(progress);
            float currentHeight = Mathf.Lerp(fromHeight, toHeight, curvedProgress);

            SetBarsHeight(currentHeight);

            yield return null;
        }

        SetBarsHeight(toHeight);
    }

    // Устанавливает высоту черных полос
    public void SetBarsHeight(float height)
    {
        if (topBlackBar == null || bottomBlackBar == null)
        {
            Debug.LogWarning("Cannot set bars height - black bars are null");
            return;
        }

        float screenHeight = Screen.height;
        float barHeight = screenHeight * height;

        topBlackBar.rectTransform.sizeDelta = new Vector2(topBlackBar.rectTransform.sizeDelta.x, barHeight);
        bottomBlackBar.rectTransform.sizeDelta = new Vector2(bottomBlackBar.rectTransform.sizeDelta.x, barHeight);
    }

    // Отписываемся от события при уничтожении
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}