using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioSource theMusic;
    public bool startPlaying;
    public BeatScroller theBS;
    public static GameManager instance;
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

    public GameObject resultsScreen;
    public Text percentHitText, rankText, normalText, goodText, perfectText, missedText, finalScoreText;

    public int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThreshold;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            if (!theMusic.isPlaying && !resultsScreen.activeInHierarchy)
            {
                resultsScreen.SetActive(true);

                normalText.text = "" + NormalHitsCount;
                goodText.text = "" + GoodHitsCount;
                perfectText.text = "" + PerfectHitsCount;
                missedText.text = "" + MissedHitsCount;

                float percentHit = Mathf.Round(((NormalHitsCount + GoodHitsCount + PerfectHitsCount) / totalNotes) * 100f);
                percentHitText.text = "" + percentHit + "%";

                string rankValue = "F";
                if(percentHit > 40)
                {
                    rankValue = "D";
                    if(percentHit > 60)
                    {
                        rankValue = "C";
                        if(percentHit > 75)
                        { 
                            rankValue = "B";
                            if(percentHit > 85)
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
            }
        }
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
