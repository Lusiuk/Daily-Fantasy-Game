using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;
   //public GameObject LoadCanvas;
    public List<GameObject> Levels;

    public List<GameObject> StartingPositions;
    private int currentLevelIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LoadNextLevel()
    {
        int nextLevelIndex = (currentLevelIndex == Levels.Count - 1) ? 0 : currentLevelIndex + 1;

        Levels[currentLevelIndex].gameObject.SetActive(false);
        Levels[nextLevelIndex].gameObject.SetActive(true);

        player.transform.position = StartingPositions[nextLevelIndex].transform.position;
        currentLevelIndex = nextLevelIndex;
    }
}

