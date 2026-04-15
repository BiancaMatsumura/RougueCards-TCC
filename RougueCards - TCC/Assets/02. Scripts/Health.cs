using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int playerID;

    public int maxHealth = 100;
    public int currentHealth;

    public event Action<int, int, int> OnHealthChanged;
    public event Action OnDeath; // 👈 NOVO

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
        OnDeath?.Invoke(); // 👈 dispara evento
        Destroy(gameObject); // melhor que SetActive(false)
    }
}