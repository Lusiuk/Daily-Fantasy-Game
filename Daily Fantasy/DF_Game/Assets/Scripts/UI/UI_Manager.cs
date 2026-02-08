using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{

    public GameObject SettingsPanel;

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

    }

    public void Exit()
    {
        Application.Quit();
    }
}
