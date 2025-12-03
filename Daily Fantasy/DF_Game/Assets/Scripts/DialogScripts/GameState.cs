using UnityEngine;

public static class GameState
{
    public static string PreviousScene { get; set; } = "";
    public static string InitialScene { get; set; } = "";
    public static bool IsFirstLoad { get; set; } = true;

    public static bool ShouldTeleport { get; set; } = false;
    public static Vector2 TeleportPosition { get; set; } = Vector2.zero;
    public static string TeleportMarkerName { get; set; } = "";
}