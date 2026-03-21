using UnityEngine;

public static class GameState
{
    public static string PreviousScene { get; set; } = "";
    public static string InitialScene { get; set; } = "";
    public static bool IsFirstLoad { get; set; } = true;

    public static bool ShouldTeleport { get; set; } = false;
    public static Vector2 TeleportPosition { get; set; } = Vector2.zero;
    public static string TeleportMarkerName { get; set; } = "";

    // Для мини-игр
    public static bool IsMinigameCompleted { get; set; } = false;
    public static string MinigameName { get; set; } = "IDEMinigame";

    // Двери платформинга
    public static bool IsDoor1Completed { get; set; } = false;
    public static bool IsDoor2Completed { get; set; } = false;
    public static bool IsDoor3Completed { get; set; } = false;

    // Метод для сброса состояний
    public static void ResetGameState()
    {
        IsFirstLoad = true;
        ShouldTeleport = false;
        TeleportPosition = Vector2.zero;
        TeleportMarkerName = "";
        IsMinigameCompleted = false;
        MinigameName = "IDEMinigame";

        IsDoor1Completed = false;
        IsDoor2Completed = false;
        IsDoor3Completed = false;

        Debug.Log("GameState: Состояние игры сброшено");
    }

    // Ключ для сохранения
    private const string SAVE_KEY = "GameState";

    // Сохранить состояние
    public static void Save()
    {
        SaveData data = new SaveData
        {
            previousScene = PreviousScene,
            initialScene = InitialScene,
            isFirstLoad = IsFirstLoad,
            isMinigameCompleted = IsMinigameCompleted,
            minigameName = MinigameName,
            isDoor1Completed = IsDoor1Completed,
            isDoor2Completed = IsDoor2Completed,
            isDoor3Completed = IsDoor3Completed
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("GameState: Сохранено");
    }

    // Загрузить состояние
    public static void Load()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            PreviousScene = data.previousScene;
            InitialScene = data.initialScene;
            IsFirstLoad = data.isFirstLoad;
            IsMinigameCompleted = data.isMinigameCompleted;
            MinigameName = data.minigameName;
            IsDoor1Completed = data.isDoor1Completed;
            IsDoor2Completed = data.isDoor2Completed;
            IsDoor3Completed = data.isDoor3Completed;

            Debug.Log("GameState: Загружено");
        }
        else
        {
            Debug.Log("GameState: Нет сохранений, используем значения по умолчанию");
        }
    }

    // Сброс сохранения
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("GameState: Сохранение удалено");
    }

    // Структура данных для сохранения
    [System.Serializable]
    private class SaveData
    {
        public string previousScene;
        public string initialScene;
        public bool isFirstLoad;
        public bool isMinigameCompleted;
        public string minigameName;
        public bool isDoor1Completed;
        public bool isDoor2Completed;
        public bool isDoor3Completed;
    }
}