using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance = null;
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
