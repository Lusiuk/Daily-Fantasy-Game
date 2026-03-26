using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallWait = 2f;
    public float destroyWait = 1f;   // по смыслу теперь это "hideWait"
    public float restoreWait = 5f;

    private bool isFalling;
    private bool isRestoring;

    private Rigidbody2D rb;
    private Collider2D col;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        startPos = transform.position;
        startRot = transform.rotation;

        // на всякий случай
        rb.bodyType = RigidbodyType2D.Static;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling && !isRestoring && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallAndRestore());
        }
    }

    private IEnumerator FallAndRestore()
    {
        isFalling = true;

        // ждём перед падением
        yield return new WaitForSeconds(fallWait);

        // падаем
        rb.bodyType = RigidbodyType2D.Dynamic;

        // даём упасть/улететь и затем "убираем" платформу
        yield return new WaitForSeconds(destroyWait);

        // "убираем", но НЕ Destroy (иначе нечего восстанавливать)
        col.enabled = false;
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        isFalling = false;

        // восстановление через restoreWait секунд (после того как платформа уже упала)
        isRestoring = true;
        yield return new WaitForSeconds(restoreWait);

        // вернуть на старт
        transform.position = startPos;
        transform.rotation = startRot;

        // вернуть состояние
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;
        col.enabled = true;
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = true;

        isRestoring = false;
    }
}