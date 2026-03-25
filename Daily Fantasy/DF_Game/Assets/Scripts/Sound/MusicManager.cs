using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Настройки аудио")]
    public AudioSource audioSource;

    // Ключ - имя сцены, Значение - клип
    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
    }

    [Header("Музыка для сцен")]
    public List<SceneMusic> musicList = new List<SceneMusic>();

    private float currentVolume = 1f;
    private string volumeKey = "MusicVolume";

    void Awake()
    {
        // Паттерн Синглтон
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать при смене сцены
            LoadVolume();
            PlayMusicForCurrentScene();
        }
        else
        {
            Destroy(gameObject); // Уничтожить дубликат
            return;
        }

        // Подписка на смену сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Вызывается автоматически при загрузке любой сцены
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        AudioClip clipToPlay = null;

        // Ищем трек для текущей сцены
        foreach (var sceneMusic in musicList)
        {
            if (sceneMusic.sceneName == currentSceneName)
            {
                clipToPlay = sceneMusic.clip;
                break;
            }
        }

        if (clipToPlay != null)
        {
            PlayMusic(clipToPlay);
        }
        else
        {
            // Если для сцены нет музыки, можно остановить или играть дефолтную
            StopMusic();
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying) return;

        audioSource.clip = clip;
        audioSource.Play();
        audioSource.volume = currentVolume;

        // Можно добавить плавное появление (Fade In) здесь
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    // Метод для изменения громкости (вызывается из UI)
    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        audioSource.volume = currentVolume;
        SaveVolume();
    }

    public float GetVolume()
    {
        return currentVolume;
    }

    void SaveVolume()
    {
        PlayerPrefs.SetFloat(volumeKey, currentVolume);
        PlayerPrefs.Save();
    }

    void LoadVolume()
    {
        currentVolume = PlayerPrefs.GetFloat(volumeKey, 1f); // 1f по умолчанию
        if (audioSource != null)
            audioSource.volume = currentVolume;
    }
}