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

    private Animator animator;

    public bool needQuickAccess = false; // Set to true if you want the level to load immediately without holding

    private HoldToLoadLevel loadCanvasInstance;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadCanvasInstance = player.transform.Find("LoadCanvas")?.gameObject?.GetComponent<HoldToLoadLevel>();
        animator = player.GetComponent<Animator>();
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
        loadCanvasInstance.needQuickAccess = needQuickAccess;
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
        animator.SetBool("dead", true);
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }

    private void OnDestroy()
    {
        HoldToLoadLevel.OnHoldComplete -= LoadNextLevel;
        PlayerPlatformingHealth.OnPlayerDied -= GameOverScreen;
    }

    public void ResetGame()
    {
        Debug.Log("ResetGame called");
        player.GetComponent<Rigidbody2D>().linearVelocityY = 0f;
        Time.timeScale = 1;
        animator.SetBool("dead", false);
        animator.Play("Idle", -1, 0f);
        gameOverScreen.SetActive(false);
        Debug.Log($"Loading level 0, Levels count: {Levels.Count}");
        LoadLevel(0);
        Debug.Log($"Player position after load: {player.transform.position}");
        OnReset?.Invoke();
    }
}

