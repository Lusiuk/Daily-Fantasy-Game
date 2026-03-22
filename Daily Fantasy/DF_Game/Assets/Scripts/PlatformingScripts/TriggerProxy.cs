using UnityEngine;

public class TriggerProxy : MonoBehaviour
{
    public HoldToLoadLevel holdScript;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {   
            holdScript.SetPlayerInside(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            holdScript.SetPlayerInside(true);
        }
    }
}