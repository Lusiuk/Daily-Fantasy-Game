using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public bool canBePressed;
    public KeyCode keyToPress;
    private bool wasHit = false;

    public GameObject HitEffect, GoodEffect, PerfectEffect, MissEffect;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            if (canBePressed && !wasHit)
            {
                wasHit = true;
                gameObject.SetActive(false);

                //GameManager.instance.NoteHits();

                if (Mathf.Abs(transform.position.y) > 0.25)
                {
                    Debug.Log("Normal Hit");
                    GameManager.instance.NormalHits();
                    Instantiate(HitEffect, transform.position, HitEffect.transform.rotation);
                }
                else if (Mathf.Abs(transform.position.y) > 0.05f)
                {
                    Debug.Log("Good Hit");
                    GameManager.instance.GoodHits();
                    Instantiate(GoodEffect, transform.position, GoodEffect.transform.rotation);
                }
                else
                {
                    Debug.Log("Perfect Hit");
                    GameManager.instance.PerfectHits();
                    Instantiate(PerfectEffect, transform.position, PerfectEffect.transform.rotation);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Activator")
        {
            canBePressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Activator")
        {
            canBePressed = false;


            if (!wasHit)
            {
                GameManager.instance.NoteMiss();
                Instantiate(MissEffect, transform.position, MissEffect.transform.rotation);
            }
        }
    }
}
