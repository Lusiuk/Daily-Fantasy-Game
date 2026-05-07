using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{

    public GameObject SettingsPanel;
    public GameObject PauseMenu;

    public DialogueSystem dialogueSystem;

    public static bool isPaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        AudioListener.volume = 1f;
        AudioListener.pause = false;

        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
    }

    public void Play()
    {
        AudioListener.volume = 1f;
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
        GameState.ResetGameState();
        SceneManager.LoadScene("CharacterRoom");
    }

    public static bool IsPaused()
    {
        if (isPaused) 
            return true;
        return false;
    }

    public void Back()
    {
        DialogueSystem.Instance?.StopMusic();
        AudioListener.volume = 0f;
        AudioListener.pause = true;

        if (isPaused)
            ResumeGame();

        SceneManager.LoadScene("MainMenu");
    }

    public void Back_MG()
    {
        DialogueSystem.Instance?.StopMusic();
        AudioListener.volume = 0f;
        AudioListener.pause = true;

        if (isPaused)
            ResumeGame();
        string current = SceneManager.GetActiveScene().name;

        if (current == "IdeMiniGame 1")
            SceneManager.LoadScene("CharacterRoom");

        if (current == "Rhythm Game")
            SceneManager.LoadScene("Street");
    }

    public void Settings()
    {
        if (!SettingsPanel.activeSelf)
        {
            SettingsPanel.SetActive(true);
        }
        else
        {
            SettingsPanel.SetActive(false);
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if(SettingsPanel.activeSelf)
                {
                    SettingsPanel.SetActive(false);
                }
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        PauseMenu.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        isPaused = true;
    }

    public void ResumeGame()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
    }
}
