using System;
using System.Collections;
using UnityEngine;

public class PlayerPlatformingHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    public HealthUI healthUI;

    public static event Action OnPlayerDied;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetHealth();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameController.OnReset += ResetHealth;
        OnPlayerDied += ResetHealth;
    }

    private void OnDestroy()
    {
        GameController.OnReset -= ResetHealth;
        OnPlayerDied -= ResetHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Trap trap = collision.GetComponent<Trap>();
        if (trap && trap.damage > 0)
        {
            TakeDamage(trap.damage);
        }
    }

    private void TakeDamage(int damage)
    {
        Debug.Log($"TakeDamage called, damage={damage}, currentHealth before={currentHealth}");

        currentHealth -= damage;

        if (healthUI != null) healthUI.UpdateHearts(currentHealth);
        else Debug.LogWarning("healthUI is NULL in PlayerPlatformingHealth");

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Debug.Log("Player died -> OnPlayerDied invoked");
            OnPlayerDied?.Invoke();
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
        yield return new WaitForEndOfFrame();
    }

    private void ResetHealth()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
    }
}
