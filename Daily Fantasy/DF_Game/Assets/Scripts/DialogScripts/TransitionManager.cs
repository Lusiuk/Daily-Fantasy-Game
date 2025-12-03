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

        // Для всех остальных загрузок сцен
        if (topBlackBar != null && bottomBlackBar != null)
        {
            SetBarsHeight(0.5f);
        }

        StartCoroutine(CompleteTransitionAfterDelay());
    }

    private IEnumerator CompleteTransitionAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

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