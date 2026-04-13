using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int playerID; // 1 ou 2

    public int maxHealth = 100;
    public int currentHealth;

    public event Action<int, int, int> OnHealthChanged; 
    // (playerID, current, max)

    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"Player {playerID} morreu");
        gameObject.SetActive(false);
    }
}