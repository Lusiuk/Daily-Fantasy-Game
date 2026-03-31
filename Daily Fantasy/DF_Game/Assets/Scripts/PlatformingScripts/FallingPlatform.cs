using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallWait = 2f;
    public float destroyWait = 1f;
    public float restoreWait = 5f;

    private bool isFalling;
    private bool isRestoring;

    private Rigidbody2D rb;
    private Collider2D col;

    private Vector3 startPos;
    private Quaternion startRot;

    private Coroutine fallRoutine;

    void Start()
    {
        PlayerPlatformingHealth.OnPlayerDied += Restore;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        startPos = transform.position;
        startRot = transform.rotation;

        rb.bodyType = RigidbodyType2D.Static;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling && !isRestoring && collision.gameObject.CompareTag("Player"))
        {
            fallRoutine = StartCoroutine(FallAndRestore());
        }
    }

    private void OnDestroy()
    {
        PlayerPlatformingHealth.OnPlayerDied -= Restore;
    }

    private IEnumerator FallAndRestore()
    {
        isFalling = true;

        yield return new WaitForSeconds(fallWait);
        rb.bodyType = RigidbodyType2D.Dynamic;

        yield return new WaitForSeconds(destroyWait);

        col.enabled = false;
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        isFalling = false;
        isRestoring = true;

        yield return new WaitForSeconds(restoreWait);

        Restore();
        isRestoring = false;
    }

    private void Restore()
    {
        if (fallRoutine != null)
        {
            StopCoroutine(fallRoutine);
            fallRoutine = null;
        }

        isFalling = false;
        isRestoring = false;

        transform.position = startPos;
        transform.rotation = startRot;

        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;
        col.enabled = true;

        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = true;
    }
}