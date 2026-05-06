using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Паузы")]
    [Tooltip("Панель меню паузы (найдется автоматически в сцене, если не назначена)")]
    public GameObject pauseMenuPanel;
    
    [Tooltip("Имя объекта панели паузы в сцене (должно совпадать во всех сценах)")]
    public string pausePanelObjectName = "PauseMenuPanel";

    [Header("Настройки")]
    [Tooltip("Имя сцены главного меню (здесь пауза не работает)")]
    public string mainMenuSceneName = "MainMenu";
    [Tooltip("Блокировать ли курсор мыши в игре (для 3D/шутеров)")]
    public bool lockCursorInGame = true;
    [Tooltip("Режим блокировки курсора")]
    public CursorLockMode cursorLockMode = CursorLockMode.Locked;
    [Tooltip("Скрывать ли курсор в игре")]
    public bool hideCursorInGame = true;

    private bool isPaused = false;
    private string lastSceneName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ PauseManager создан и сохранён между сценами");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ищем панель в первой сцене
        FindPausePanelInScene();

        UpdateCursorState();
    }

    void Start()
    {
        lastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateCursorState();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastSceneName = scene.name;

        // Если пауза была активна при загрузке сцены — закрываем её
        if (isPaused)
            Unpause();

        // 🔍 Ищем панель в новой сцене
        FindPausePanelInScene();
    }

    // === НОВЫЙ МЕТОД: Поиск панели в текущей сцене ===
    void FindPausePanelInScene()
    {
        // Пробуем найти панель по имени объекта
        GameObject panelObj = GameObject.Find(pausePanelObjectName);

        if (panelObj != null)
        {
            pauseMenuPanel = panelObj;
            pauseMenuPanel.SetActive(false); // Скрываем при загрузке
            Debug.Log("✅ Панель паузы найдена в сцене: " + SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogWarning("⚠️ Панель паузы '" + pausePanelObjectName + "' не найдена в сцене!");
            Debug.LogWarning("   Создай объект с таким именем в сцене или назначь вручную в Inspector.");
        }
    }

    void Update()
    {
        // Не реагировать на ESC в главном меню
        string currentScene = SceneManager.GetActiveScene().name;
        if (string.Equals(currentScene, mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase))
            return;

        // Реакция на ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Unpause();
            else
                Pause();
        }
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ===

    public void Pause()
    {
        if (pauseMenuPanel == null)
        {
            Debug.LogError("❌ ОШИБКА: pauseMenuPanel не назначен! Проверь наличие объекта '" + pausePanelObjectName + "' в сцене.");
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("🔵 Игра на паузе");
    }

    public void Unpause()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        UpdateCursorState();
        Debug.Log("🟢 Игра продолжена");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(lastSceneName);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    void UpdateCursorState()
    {
        if (lockCursorInGame)
            Cursor.lockState = cursorLockMode;
        else
            Cursor.lockState = CursorLockMode.None;

        Cursor.visible = !hideCursorInGame;
    }

    public bool IsPaused() => isPaused;
}