using System.Collections;
using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform creditsRect;   // RectTransform текста титров
    [SerializeField] private GameObject thanksPanel;      // Панель "Спасибо за игру"
    [SerializeField] private float scrollSpeed = 60f;     // пикселей в секунду

    [Header("Scroll positions (anchored Y)")]
    [SerializeField] private float startY = -600f;        // откуда стартуем (ниже экрана)
    [SerializeField] private float endY = 1200f;          // куда доезжаем (выше экрана)

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource musicSource;

    private void OnEnable()
    {
        if (thanksPanel != null)
            thanksPanel.SetActive(false);

        // Ставим текст в стартовую позицию
        var p = creditsRect.anchoredPosition;
        creditsRect.anchoredPosition = new Vector2(p.x, startY);

        if (musicSource != null)
            musicSource.Play();

        StartCoroutine(ScrollRoutine());
    }

    private IEnumerator ScrollRoutine()
    {
        while (creditsRect.anchoredPosition.y < endY)
        {
            creditsRect.anchoredPosition += Vector2.up * (scrollSpeed * Time.deltaTime);
            yield return null;
        }

        // Показать финальную панель
        if (thanksPanel != null)
            thanksPanel.SetActive(true);
    }
}