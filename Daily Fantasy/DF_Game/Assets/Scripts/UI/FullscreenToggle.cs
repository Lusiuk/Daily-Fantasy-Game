using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggleController : MonoBehaviour
{
    [Tooltip("Ссылка на Toggle в сцене")]
    public Toggle fullscreenToggle;

    void Start()
    {
        // Получаем ссылку на Toggle, если не задана в инспекторе
        if (fullscreenToggle == null)
            fullscreenToggle = GetComponent<Toggle>();

        // Загружаем сохранённое значение (если есть)
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = isFullscreen;

        // Применяем текущее состояние
        SetFullscreen(isFullscreen);

        // Подписываемся на изменение
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
    }

    void OnFullscreenToggled(bool isOn)
    {
        SetFullscreen(isOn);
        PlayerPrefs.SetInt("Fullscreen", isOn ? 1 : 0);
        PlayerPrefs.Save(); // Сохраняем сразу (важно!)
    }

    void SetFullscreen(bool enable)
    {
        if (enable)
        {
            // Полноэкранный — фиксируем разрешение (можно взять текущее)
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            // Оконный — оставляем текущее разрешение, но разрешаем изменять размер
            Screen.fullScreenMode = FullScreenMode.Windowed;
            // НЕ вызывай Screen.SetResolution() здесь — иначе окно "заморозится"
        }
    }

    void OnDestroy()
    {
        // Отписываемся, чтобы избежать утечек
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
    }
}