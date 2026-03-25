using UnityEngine;
using UnityEngine.UI;

public class UIVolumeController : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        if (volumeSlider != null)
        {
            // Устанавливаем значение ползунка согласно сохраненной громкости
            volumeSlider.value = MusicManager.Instance.GetVolume();

            // Подписываемся на изменение ползунка
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    void OnVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(value);
        }
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}