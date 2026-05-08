using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    // Состояния сундука
    private enum ChestState { Closed, Opening, Open, Closing }
    private ChestState currentState = ChestState.Closed;

    // Компоненты
    private Animator animator;
    private Canvas easterEggCanvas;
    private CanvasGroup canvasGroup;

    // Параметры анимации
    [SerializeField] public string openAnimationTrigger = "Open";
    [SerializeField] public string closeAnimationTrigger = "Close";
    [SerializeField] public float animationDuration = 1f;

    // Параметры UI
    public GameObject easterEggPrefab;
    [SerializeField] public float fadeInDuration = 0.5f;
    [SerializeField] public float fadeOutDuration = 0.5f;

    private bool isPlayerNearby = false;
    private GameObject currentEasterEggInstance = null;

    private void Start()
    {
        Debug.Log("[CHEST] === START ФУНКЦИЯ ===");
        
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            Debug.Log("[CHEST] ✓ Animator найден!");
        }
        else
        {
            Debug.LogError("[CHEST] ❌ Animator НЕ найден! Проверьте сундук.");
        }

        Debug.Log("[CHEST] === START ЗАВЕРШЕНА ===\n");
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[CHEST] R НАЖАТА! Текущее состояние: " + currentState);
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        Debug.Log("[CHEST] === HANDLE INTERACTION ===");
        Debug.Log("[CHEST] Текущее состояние: " + currentState);
        
        switch (currentState)
        {
            case ChestState.Closed:
                Debug.Log("[CHEST] Состояние Closed → запускаю OpenChest");
                StartCoroutine(OpenChest());
                break;
            case ChestState.Open:
                Debug.Log("[CHEST] Состояние Open → запускаю CloseChest");
                StartCoroutine(CloseChest());
                break;
            case ChestState.Opening:
                Debug.Log("[CHEST] ⚠️ Состояние Opening → игнорирую (сундук уже открывается)");
                break;
            case ChestState.Closing:
                Debug.Log("[CHEST] ⚠️ Состояние Closing → игнорирую (сундук уже закрывается)");
                break;
        }
    }

    private IEnumerator OpenChest()
    {
        Debug.Log("[CHEST] === OPEN CHEST НАЧАЛО ===");
        currentState = ChestState.Opening;
        Debug.Log("[CHEST] Состояние изменено на: " + currentState);

        // Воспроизводим анимацию открытия
        if (animator != null)
        {
            Debug.Log("[CHEST] Запускаю анимацию открытия с триггером: '" + openAnimationTrigger + "'");
            animator.SetTrigger(openAnimationTrigger);
            
            Debug.Log("[CHEST] Жду " + animationDuration + " секунд завершения анимации...");
            yield return new WaitForSeconds(animationDuration);
            Debug.Log("[CHEST] ✓ Анимация открытия завершена");
        }
        else
        {
            Debug.LogError("[CHEST] ❌ Animator NULL! Не могу воспроизвести анимацию открытия");
        }

        currentState = ChestState.Open;
        Debug.Log("[CHEST] Состояние изменено на: " + currentState);

        // Показываем пасхалку
        if (easterEggPrefab != null)
        {
            Debug.Log("[CHEST] Создаю экземпляр пасхалки...");
            currentEasterEggInstance = Instantiate(easterEggPrefab);
            
            easterEggCanvas = currentEasterEggInstance.GetComponent<Canvas>();
            if (easterEggCanvas == null)
            {
                Debug.LogError("[CHEST] ❌ Canvas не найден на пасхалке!");
            }
            else
            {
                Debug.Log("[CHEST] ✓ Canvas найден!");
            }

            canvasGroup = currentEasterEggInstance.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.Log("[CHEST] CanvasGroup не найден, создаю новый...");
                canvasGroup = currentEasterEggInstance.AddComponent<CanvasGroup>();
                Debug.Log("[CHEST] ✓ CanvasGroup создан!");
            }

            canvasGroup.alpha = 0f;
            
            Debug.Log("[CHEST] Запускаю FadeIn пасхалки с длительностью " + fadeInDuration);
            yield return StartCoroutine(FadeIn(canvasGroup, fadeInDuration));
            Debug.Log("[CHEST] ✓ FadeIn завершен, пасхалка видна");
        }
        else
        {
            Debug.LogError("[CHEST] ❌ easterEggPrefab НЕ назначен в Inspector!");
        }
        
        Debug.Log("[CHEST] === OPEN CHEST ЗАВЕРШЕНО ===\n");
    }

    private IEnumerator CloseChest()
    {
        Debug.Log("[CHEST] === CLOSE CHEST НАЧАЛО ===");
        currentState = ChestState.Closing;
        Debug.Log("[CHEST] Состояние изменено на: " + currentState);

        // Скрываем пасхалку
        if (canvasGroup != null)
        {
            Debug.Log("[CHEST] Запускаю FadeOut пасхалки с длительностью " + fadeOutDuration);
            yield return StartCoroutine(FadeOut(canvasGroup, fadeOutDuration));
            Debug.Log("[CHEST] ✓ FadeOut завершен, пасхалка скрыта");
        }
        else
        {
            Debug.LogWarning("[CHEST] ⚠️ CanvasGroup NULL при закрытии");
        }

        // Удаляем пасхалку
        if (currentEasterEggInstance != null)
        {
            Debug.Log("[CHEST] Удаляю экземпляр пасхалки");
            Destroy(currentEasterEggInstance);
            currentEasterEggInstance = null;
            canvasGroup = null;
        }

        // Воспроизводим анимацию закрытия
        if (animator != null)
        {
            Debug.Log("[CHEST] Запускаю анимацию закрытия с триггером: '" + closeAnimationTrigger + "'");
            animator.SetTrigger(closeAnimationTrigger);
            
            Debug.Log("[CHEST] Жду " + animationDuration + " секунд завершения анимации...");
            yield return new WaitForSeconds(animationDuration);
            Debug.Log("[CHEST] ✓ Анимация закрытия завершена");
        }
        else
        {
            Debug.LogError("[CHEST] ❌ Animator NULL! Не могу воспроизвести анимацию закрытия");
        }

        currentState = ChestState.Closed;
        Debug.Log("[CHEST] Состояние изменено на: " + currentState);
        Debug.Log("[CHEST] === CLOSE CHEST ЗАВЕРШЕНО ===\n");
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        Debug.Log("[CHEST] [FADEIN] Начало FadeIn. Длительность: " + duration + ", Начальная alpha: " + canvasGroup.alpha);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            Debug.Log("[CHEST] [FADEIN] elapsed: " + elapsed.ToString("F2") + "s, alpha: " + canvasGroup.alpha.ToString("F2"));
            yield return null;
        }

        canvasGroup.alpha = 1f;
        Debug.Log("[CHEST] [FADEIN] ✓ Завершено. Финальная alpha: " + canvasGroup.alpha);
    }

    private IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        Debug.Log("[CHEST] [FADEOUT] Начало FadeOut. Длительность: " + duration + ", Начальная alpha: " + canvasGroup.alpha);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            Debug.Log("[CHEST] [FADEOUT] elapsed: " + elapsed.ToString("F2") + "s, alpha: " + canvasGroup.alpha.ToString("F2"));
            yield return null;
        }

        canvasGroup.alpha = 0f;
        Debug.Log("[CHEST] [FADEOUT] ✓ Завершено. Финальная alpha: " + canvasGroup.alpha);
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("[CHEST] [TRIGGER] Что-то вошло в триггер: " + collision.gameObject.name);
        
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("[CHEST] ✓ ИГРОК РЯДОМ! Можно нажимать R");
        }
        else
        {
            Debug.Log("[CHEST] ⚠️ Объект не имеет тега 'Player'. Теги: " + collision.tag);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        Debug.Log("[CHEST] [TRIGGER] Что-то вышло из триггера: " + collision.gameObject.name);
        
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            Debug.Log("[CHEST] Игрок уходит от сундука");

            if (currentState == ChestState.Open)
            {
                Debug.Log("[CHEST] Сундук был открыт, автоматически закрываю...");
                StartCoroutine(CloseChest());
            }
            else
            {
                Debug.Log("[CHEST] Сундук был в состоянии: " + currentState);
            }
        }
    }
}
