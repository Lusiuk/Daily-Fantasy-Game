using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    public float beatTempo; // The tempo of the beats in beats per minute (BPM)

    public bool hasStarted; // Flag to indicate if the beat scroller has started

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        beatTempo = beatTempo / 60f; // Convert BPM to beats per second
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasStarted)
        {
            //if(Input.anyKeyDown)
            //{
            //    hasStarted = true;
            //}
        }
        else
        {
            transform.position -= new Vector3(0f, beatTempo * Time.deltaTime, 0f);
        }
    }
}
