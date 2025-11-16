using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool autoTrigger = false;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float promptOffset = 1f;

    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private Transform playerTransform;

    // Start вызывается перед первым кадром
    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    // OnTriggerEnter2D вызывается когда другой Collider2D входит в триггер
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;

            if (interactionPrompt != null && !hasBeenUsed)
            {
                interactionPrompt.SetActive(true);
                UpdatePromptPosition();
            }

            if (autoTrigger && !hasBeenUsed)
            {
                TriggerDialogue();
            }
        }
    }

    // OnTriggerExit2D вызывается когда другой Collider2D выходит из триггера
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    // Update вызывается каждый кадр
    void Update()
    {
        if (playerInRange && interactionPrompt != null && interactionPrompt.activeSelf)
        {
            UpdatePromptPosition();
        }

        if (playerInRange && !autoTrigger && Input.GetKeyDown(KeyCode.R) && !hasBeenUsed)
        {
            TriggerDialogue();
        }
    }

    /// <summary>
    /// Обновляет позицию подсказки над игроком
    /// </summary>
    private void UpdatePromptPosition()
    {
        if (playerTransform != null)
        {
            Vector3 promptPosition = playerTransform.position + Vector3.up * promptOffset;
            interactionPrompt.transform.position = promptPosition;
        }
    }

    /// <summary>
    /// Запускает диалог
    /// </summary>
    public void TriggerDialogue()
    {
        if (dialogue == null || hasBeenUsed) return;

        DialogueSystem.Instance.ShowDialogue(dialogue);

        if (oneTimeUse)
        {
            hasBeenUsed = true;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Сбрасывает состояние триггера
    /// </summary>
    public void ResetTrigger()
    {
        hasBeenUsed = false;
        if (interactionPrompt != null && playerInRange)
        {
            interactionPrompt.SetActive(true);
        }
    }

    // OnDrawGizmos вызывается каждый кадр в редакторе для отрисовки гизмо
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "dialogue_icon.png", true);
    }

    // OnDrawGizmosSelected вызывается только когда объект выделен в редакторе
    void OnDrawGizmosSelected()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawCube(transform.position, collider.bounds.size);
        }
    }
}