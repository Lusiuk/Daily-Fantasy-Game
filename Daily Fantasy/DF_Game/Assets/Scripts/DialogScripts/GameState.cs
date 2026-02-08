using UnityEngine;

public static class GameState
{
    public static string PreviousScene { get; set; } = "";
    public static string InitialScene { get; set; } = "";
    public static bool IsFirstLoad { get; set; } = true;

    public static bool ShouldTeleport { get; set; } = false;
    public static Vector2 TeleportPosition { get; set; } = Vector2.zero;
    public static string TeleportMarkerName { get; set; } = "";

    public static bool IsMinigameCompleted { get; set; } = false;
    public static string MinigameName { get; set; } = "IDEMinigame";

    // Метод для сброса состояний
    public static void ResetGameState()
    {
        IsFirstLoad = true;
        ShouldTeleport = false;
        TeleportPosition = Vector2.zero;
        TeleportMarkerName = "";
        IsMinigameCompleted = false;
        MinigameName = "IDEMinigame";

        Debug.Log("GameState: Состояние игры сброшено");
    }
}