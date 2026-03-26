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
    public static string MinigameName { get; set; } = "IDEMinigame";

    // IDE
    public static bool IsIDEMinigameCompleted { get; set; } = false;

    // Двери платформинга
    public static bool IsDoor1Completed { get; set; } = false;
    public static bool IsDoor2Completed { get; set; } = false;
    public static bool IsDoor3Completed { get; set; } = false;

    // Ритм-игры
    public static bool IsRhythmGame1Completed { get; set; } = false;
    public static bool IsRhythmGame2Completed { get; set; } = false;

    // Метод для сброса состояний
    public static void ResetGameState()
    {
        IsFirstLoad = true;
        ShouldTeleport = false;
        TeleportPosition = Vector2.zero;
        TeleportMarkerName = "";
        IsIDEMinigameCompleted = false;

        IsDoor1Completed = false;
        IsDoor2Completed = false;
        IsDoor3Completed = false;

        IsRhythmGame1Completed = false;
        IsRhythmGame2Completed = false;

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
            isIDEMinigameCompleted = IsIDEMinigameCompleted,
            minigameName = MinigameName,
            isDoor1Completed = IsDoor1Completed,
            isDoor2Completed = IsDoor2Completed,
            isDoor3Completed = IsDoor3Completed,
            isRhythmGame1Completed = IsRhythmGame1Completed,
            isRhythmGame2Completed = IsRhythmGame2Completed
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
            IsIDEMinigameCompleted = data.isIDEMinigameCompleted;
            MinigameName = data.minigameName;
            IsDoor1Completed = data.isDoor1Completed;
            IsDoor2Completed = data.isDoor2Completed;
            IsDoor3Completed = data.isDoor3Completed;
            IsRhythmGame1Completed = data.isRhythmGame1Completed;
            IsRhythmGame2Completed = data.isRhythmGame2Completed;

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
        public bool isIDEMinigameCompleted;
        public string minigameName;
        public bool isDoor1Completed;
        public bool isDoor2Completed;
        public bool isDoor3Completed;
        public bool isRhythmGame1Completed;
        public bool isRhythmGame2Completed;
    }

    // Получить значение флага по имени
    public static bool GetFlag(string flagName)
    {
        switch (flagName)
        {
            case "IsIDEMinigameCompleted": return IsIDEMinigameCompleted;
            case "IsDoor1Completed": return IsDoor1Completed;
            case "IsDoor2Completed": return IsDoor2Completed;
            case "IsDoor3Completed": return IsDoor3Completed;
            case "IsRhythmGame1Completed": return IsRhythmGame1Completed;
            case "IsRhythmGame2Completed": return IsRhythmGame2Completed;
            default:
                Debug.LogWarning($"Неизвестный флаг: {flagName}");
                return false;
        }
    }

    // Проверить, удовлетворены ли все требования
    public static bool AreFlagsSatisfied(string[] flags)
    {
        if (flags == null || flags.Length == 0) return true;
        foreach (string flag in flags)
        {
            if (!GetFlag(flag)) return false;
        }
        return true;
    }
}