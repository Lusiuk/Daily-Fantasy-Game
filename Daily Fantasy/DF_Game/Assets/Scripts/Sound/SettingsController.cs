using UnityEngine;
using UnityEngine.UI;

public class UIVolumeController : MonoBehaviour
{
    public Slider volumeSlider;


    void Start()
    {
        if (volumeSlider == null)
        {
            Debug.LogError("UIVolumeController: volumeSlider не назначен", this);
            enabled = false;
            return;
        }

        if (MusicManager.Instance == null)
        {
            Debug.LogError("UIVolumeController: MusicManager.Instance == null (нет MusicManager в сцене?)", this);
            enabled = false;
            return;
        }

        volumeSlider.value = MusicManager.Instance.GetVolume();
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}