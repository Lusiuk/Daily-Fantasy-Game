using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameState
{
    public static string PreviousScene { get; set; } = "";
    public static string InitialScene { get; set; } = "";
    public static bool IsFirstLoad { get; set; } = true;

    public static bool ShouldTeleport { get; set; } = false;
    public static Vector2 TeleportPosition { get; set; } = Vector2.zero;
    public static string TeleportMarkerName { get; set; } = "";

    // IDE
    private static bool _isIDEMinigameCompleted;
    public static bool IsIDEMinigameCompleted
    {
        get => _isIDEMinigameCompleted;
        set
        {
            if (_isIDEMinigameCompleted != value)
            {
                _isIDEMinigameCompleted = value;
                OnFlagChanged?.Invoke("IsIDEMinigameCompleted");
            }
        }
    }

    // Двери платформинга
    private static bool _isDoor1Completed;
    public static bool IsDoor1Completed
    {
        get => _isDoor1Completed;
        set
        {
            if (_isDoor1Completed != value)
            {
                _isDoor1Completed = value;
                OnFlagChanged?.Invoke("IsDoor1Completed");
            }
        }
    }
    private static bool _isDoor2Completed;
    public static bool IsDoor2Completed
    {
        get => _isDoor2Completed;
        set
        {
            if (_isDoor2Completed != value)
            {
                _isDoor2Completed = value;
                OnFlagChanged?.Invoke("IsDoor2Completed");
            }
        }
    }
    private static bool _isDoor3Completed;
    public static bool IsDoor3Completed
    {
        get => _isDoor3Completed;
        set
        {
            if (_isDoor3Completed != value)
            {
                _isDoor3Completed = value;
                OnFlagChanged?.Invoke("IsDoor3Completed");
            }
        }
    }
    private static bool _isPlatformingCompleted;
    public static bool IsPlatformingCompleted
    {
        get => _isPlatformingCompleted;
        set
        {
            if (_isPlatformingCompleted != value)
            {
                _isPlatformingCompleted = value;
                OnFlagChanged?.Invoke("IsPlatformingCompleted");
            }
        }
    }

    // Ритм-игры
    private static bool _isRhythmGame1Completed;
    public static bool IsRhythmGame1Completed
    {
        get => _isRhythmGame1Completed;
        set
        {
            if (_isRhythmGame1Completed != value)
            {
                _isRhythmGame1Completed = value;
                OnFlagChanged?.Invoke("IsRhythmGame1Completed");
            }
        }
    }
    private static bool _isRhythmGame2Completed;
    public static bool IsRhythmGame2Completed
    {
        get => _isRhythmGame2Completed;
        set
        {
            if (_isRhythmGame2Completed != value)
            {
                _isRhythmGame2Completed = value;
                OnFlagChanged?.Invoke("IsRhythmGame2Completed");
            }
        }
    }

    //Событие
    public static event System.Action<string> OnFlagChanged;

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
        IsPlatformingCompleted = false;

        IsRhythmGame1Completed = false;
        IsRhythmGame2Completed = false;

        _usedDialogues.Clear();

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
            isDoor1Completed = IsDoor1Completed,
            isDoor2Completed = IsDoor2Completed,
            isDoor3Completed = IsDoor3Completed,
            isPlatformingCompleted = IsPlatformingCompleted,
            isRhythmGame1Completed = IsRhythmGame1Completed,
            isRhythmGame2Completed = IsRhythmGame2Completed
        };

        data.usedDialogues = _usedDialogues.ToList();
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
            IsDoor1Completed = data.isDoor1Completed;
            IsDoor2Completed = data.isDoor2Completed;
            IsDoor3Completed = data.isDoor3Completed;
            IsPlatformingCompleted = data.isPlatformingCompleted;
            IsRhythmGame1Completed = data.isRhythmGame1Completed;
            IsRhythmGame2Completed = data.isRhythmGame2Completed;
            _usedDialogues = new HashSet<string>(data.usedDialogues);

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
        public bool isPlatformingCompleted;
        public bool isRhythmGame1Completed;
        public bool isRhythmGame2Completed;
        public List<string> usedDialogues;
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
            case "IsPlatformingCompleted": return IsPlatformingCompleted;
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
            if (!CheckCondition(flag)) return false;
        }
        return true;
    }

    private static HashSet<string> _usedDialogues = new HashSet<string>();

    public static bool IsDialogueUsed(string dialogueId)
    {
        return !string.IsNullOrEmpty(dialogueId) && _usedDialogues.Contains(dialogueId);
    }

    public static void MarkDialogueUsed(string dialogueId)
    {
        if (string.IsNullOrEmpty(dialogueId)) return;
        if (_usedDialogues.Add(dialogueId))
        {
            Debug.Log($"Диалог {dialogueId} отмечен как использованный");
            OnFlagChanged?.Invoke($"DialogueUsed:{dialogueId}");
        }
    }

    public static bool CheckCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return true;

        if (condition.StartsWith("DialogueUsed:"))
        {
            string dialogueId = condition.Substring("DialogueUsed:".Length);
            return IsDialogueUsed(dialogueId);
        }

        return GetFlag(condition);
    }
}