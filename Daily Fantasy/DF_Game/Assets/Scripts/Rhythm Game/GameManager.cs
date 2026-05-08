using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("General Settings")]
    public AudioSource theMusic;
    public bool startPlaying;
    public BeatScroller theBS;
    public static GameManager instance;
    public UI_Manager manager;

    public Dialogue completionDialogue;

    public GameObject resultButton;

    [Header("Scores and Results")]

    public int currentScore;
    public int scorePerNote = 100;
    public int scorePerGoodNote = 125;
    public int scorePerPerfectNote = 150;

    public Text scoreText;
    public Text multiText;

    public float totalNotes;
    public float NormalHitsCount;
    public float GoodHitsCount;
    public float PerfectHitsCount;
    public float MissedHitsCount;

    [Header("Finishing info")]

    public GameObject resultsScreen;
    public Text percentHitText, rankText, normalText, goodText, perfectText, missedText, finalScoreText;

    [Header("Multipliers")]
    public int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThreshold;

    private bool resultsShown = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resultButton.SetActive(false);
        resultsScreen.SetActive(false);
        instance = this;
        scoreText.text = "Очки: 0";
        currentMultiplier = 1;

        totalNotes = FindObjectsByType<NoteObject>(FindObjectsSortMode.None).Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (!startPlaying)
        {
            if (Input.anyKeyDown)
            {
                startPlaying = true;
                theBS.hasStarted = true;

                theMusic.Play();
            }
        }
        else
        {
            if (!theMusic.isPlaying && !resultsScreen.activeInHierarchy && !resultsShown && !UI_Manager.IsPaused() && !AudioListener.pause)
            {
                resultsShown = true;
                StartCoroutine(ShowResultsAfterDelay());
                return;
            }
        }
    }

    private IEnumerator ShowResultsAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        resultsScreen.SetActive(true);
        normalText.text = "" + NormalHitsCount;
        goodText.text = "" + GoodHitsCount;
        perfectText.text = "" + PerfectHitsCount;
        missedText.text = "" + MissedHitsCount;

        float percentHit = Mathf.Round(((NormalHitsCount + GoodHitsCount + PerfectHitsCount) / totalNotes) * 100f);
        percentHitText.text = "" + percentHit + "%";

        string rankValue = "F";
        if (percentHit > 40)
        {
            rankValue = "D";
            if (percentHit > 60)
            {
                rankValue = "C";
                if (percentHit > 75)
                {
                    rankValue = "B";
                    if (percentHit > 85)
                    {
                        rankValue = "A";
                        if (percentHit > 95)
                        {
                            rankValue = "S";
                        }
                    }
                }
            }
        }

        rankText.text = rankValue;
        finalScoreText.text = currentScore.ToString();
        resultButton.SetActive(true);
    }

    public void OnResultPressed()
    {
        if (resultsScreen != null)
            resultsScreen.SetActive(false);
        if (resultButton != null)
            resultButton.SetActive(false);

        // Устанавливаем флаг завершения мини-игры
        GameState.IsRhythmGame1Completed = true;
        GameState.Save();
        Debug.Log("Мини-игра отмечена как завершённая в GameState");

        // Показ диалога
        if (completionDialogue != null && DialogueSystem.Instance != null)
        {
            Debug.Log($"Calling completion dialogue: {completionDialogue.name}, timeScale={Time.timeScale}");
            DialogueSystem.Instance.ShowDialogue(completionDialogue);
        }
        else
        {
            Debug.LogWarning($"Completion dialogue NOT shown. completionDialogue={(completionDialogue ? completionDialogue.name : "null")}, DialogueSystem.Instance={(DialogueSystem.Instance ? "ok" : "null")}");
        }
    }

    public void End_game()
    {
        SceneManager.LoadScene("MainMenu");
    }


    public void NoteHits()
    {
        //Debug.Log("Hit on time!");

        if (currentMultiplier - 1 < multiplierThreshold.Length)
        {
            multiplierTracker++;

            if (multiplierThreshold[currentMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currentMultiplier++;
            }
        }

        multiText.text = "Множитель: x" + currentMultiplier;

        //currentScore += scorePerNote * currentMultiplier;
        scoreText.text = "Очки: " + currentScore;
    }

    public void NormalHits()
    {
        currentScore += scorePerNote * currentMultiplier;
        NoteHits();

        NormalHitsCount++;
    }

    public void GoodHits()
    {
        currentScore += scorePerGoodNote * currentMultiplier;
        NoteHits();

        GoodHitsCount++;
    }

    public void PerfectHits()
    {
        currentScore += scorePerPerfectNote * currentMultiplier;
        NoteHits();

        PerfectHitsCount++;
    }

    public void NoteMiss()
    {
        Debug.Log("Missed note!");
        currentMultiplier = 1;
        multiplierTracker = 0;
        multiText.text = "Множитель: x" + currentMultiplier;

        MissedHitsCount++;
    }

}
