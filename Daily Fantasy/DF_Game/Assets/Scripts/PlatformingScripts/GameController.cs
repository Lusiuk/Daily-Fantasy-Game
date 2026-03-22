using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System;

public class GameController : MonoBehaviour
{
    [Header("General Settings")]
    public GameObject player;
    //public GameObject LoadCanvas;
    public List<GameObject> Levels;

    public List<GameObject> StartingPositions;

    private int currentLevelIndex = 0;

    [Header("Camera Settings")]
    public CinemachineCamera vcam;

    private CinemachineConfiner2D confiner;

    [Header("GameOver Settings")]
    public GameObject gameOverScreen;
    
    public static event Action OnReset;


    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
        if (vcam != null)
            confiner = vcam.GetComponent<CinemachineConfiner2D>();
            
        UpdateCameraBounds();
        PlayerPlatformingHealth.OnPlayerDied += GameOverScreen;
        gameOverScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadLevel(int level)
    {
         Levels[currentLevelIndex].gameObject.SetActive(false);
        Levels[level].gameObject.SetActive(true);

        Vector3 newPos = StartingPositions[level].transform.position;
        player.transform.position = newPos;

        currentLevelIndex = level;
        UpdateCameraBounds();
    }

    private void LoadNextLevel()
    {
        int nextLevelIndex = (currentLevelIndex == Levels.Count - 1) ? 0 : currentLevelIndex + 1;
        LoadLevel(nextLevelIndex);
    }

    private void UpdateCameraBounds()
    {
        if (confiner == null || vcam == null) return;

        Collider2D newBounds = Levels[currentLevelIndex].GetComponentInChildren<PolygonCollider2D>();

        if (newBounds != null)
        {
            confiner.BoundingShape2D = newBounds;
        }

        vcam.ForceCameraPosition(player.transform.position, Quaternion.identity);
    }

    private void GameOverScreen()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResetGame()
    {
        gameOverScreen.SetActive(false);
        LoadLevel(0);
        OnReset.Invoke();
        Time.timeScale = 1;
    }
}

