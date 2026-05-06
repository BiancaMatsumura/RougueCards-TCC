using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int playerID;

    public int maxHealth = 100;
    public int currentHealth;

    public event Action<int, int, int> OnHealthChanged;
    public event Action OnDeath; // 👈 NOVO

    private Animation anim;

    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);

        GameOverWatcher.Instance?.RegisterPlayer(this);
        anim = GetComponent<Animation>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);

        if (GetComponent<Enemy>() != null && anim != null)
        {
            anim.Play("DAMAGEVFX");
        }

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke(); // 👈 dispara evento
        Destroy(gameObject); // melhor que SetActive(false)
    }
}