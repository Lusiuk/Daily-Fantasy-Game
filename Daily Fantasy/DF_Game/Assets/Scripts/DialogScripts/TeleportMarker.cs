using UnityEngine;

public class TeleportMarker : MonoBehaviour
{
    [Header("Marker Settings")]
    public string markerName = "TeleportPoint";

    [Header("Visual Settings")]
    public bool showGizmo = true;
    public Color gizmoColor = Color.green;

    private void Start()
    {
        if (string.IsNullOrEmpty(gameObject.name) || gameObject.name == "GameObject")
        {
            gameObject.name = markerName;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Рисуем стрелку направления
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
        Gizmos.DrawLine(transform.position + transform.up, transform.position + transform.up * 0.7f + transform.right * 0.3f);
        Gizmos.DrawLine(transform.position + transform.up, transform.position + transform.up * 0.7f - transform.right * 0.3f);
    }
}