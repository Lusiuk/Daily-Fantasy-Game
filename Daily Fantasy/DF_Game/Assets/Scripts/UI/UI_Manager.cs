using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{

    public GameObject SettingsPanel;
    public GameObject PauseMenu;

    public static bool isPaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
    }

    public void Play()
    {
        GameState.ResetGameState();
        SceneManager.LoadScene("CharacterRoom");
    }

    public void Back()
    {
        if (isPaused)
            ResumeGame();

        SceneManager.LoadScene("MainMenu");
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
        isPaused = true;
    }

    public void ResumeGame()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
