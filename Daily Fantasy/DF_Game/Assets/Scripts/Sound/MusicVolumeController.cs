using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeController : MonoBehaviour
{
    [Header("Ссылки")]
    public Slider musicSlider;
    public AudioSource musicSource; // музыкальный AudioSource

    [Header("Сохранение")]
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const float DEFAULT_VOLUME = 1.0f;

    void Start()
    {
        // Ссылки
        if (musicSlider == null) musicSlider = GetComponent<Slider>();
        if (musicSource == null) Debug.LogError("Не задан musicSource!");

        // Загрузка
        int savedInt = PlayerPrefs.GetInt(MUSIC_VOL_KEY, 10); // 10 = 100
        musicSlider.value = savedInt; // целое число


        // Применение
        SetMusicVolume(savedInt); // передаём 0 ... 10

        // Подписка
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    public void SetMusicVolume(float sliderValue)
    {
        // sliderValue = 0 -> 10
        // Переводим в диапазон 0.0 -> 1.0
        float volume = sliderValue / 10f;

        if (musicSource != null)
            musicSource.volume = volume;

        // Сохраняем как 0 ... 10 (удобнее для отладки)
        PlayerPrefs.SetInt(MUSIC_VOL_KEY, (int)sliderValue);
        PlayerPrefs.Save();

        // Обновляем метку
        //if (volumeLabel != null)
            //volumeLabel.text = $"Музыка: {(int)(volume * 100)}%";
    }

    void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
    }
}