using RougueCards.Attributes;
using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int playerID;
    public int maxHealth = 100;
    public int currentHealth;

    public event Action<int, int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnHit;

    public PlayerStats pStats;
    public PlayerStats lastAttacker;
    private bool _isDead = false;

    void Awake()
    {
        pStats = GetComponentInParent<PlayerStats>();
        GameOverWatcher.Instance?.RegisterPlayer(this);
        if (currentHealth <= 0) currentHealth = maxHealth;
    }

    void Start()
    {
        if (pStats != null)
            RefreshMaxHP();

        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }

    public void ForceRefresh()
    {
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }
    public void TakeDamage(int amount, PlayerStats attacker = null)
    {
        if (_isDead) return;

        if (attacker != null)
            lastAttacker = attacker;

        if (pStats != null)
        {
            float evasionChance = pStats.stats.Evasion.Value;
            if (UnityEngine.Random.Range(0f, 100f) < evasionChance)
            {
                Debug.Log($"[Health] Player {playerID} ESQUIVOU!");
                return;
            }

            float armorValue = pStats.stats.Armor.Value;
            amount = Mathf.Max(1, amount - (int)armorValue);
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
        OnHit?.Invoke();

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        lastAttacker?.AddKill();

        var downed = GetComponent<DownedState>();
        if (downed != null)
        {
            downed.OnDownedExpired += () => { OnDeath?.Invoke(); gameObject.SetActive(false); };
            downed.EnterDownedState();
        }
        else
        {
            OnDeath?.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void RefreshMaxHP()
    {
        if (pStats != null)
        {
            int oldMax = maxHealth;
            maxHealth = (int)pStats.stats.MaxHP.Value;

            if (maxHealth > oldMax && oldMax != 0)
                currentHealth += (maxHealth - oldMax);

            OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);

            if (playerID > 0)
                Debug.Log($"[Health] Player {playerID} MaxHP sincronizado: {maxHealth}");
        }
    }

    public void Revive(float hpPercent)
    {
        _isDead = false;
        currentHealth = (int)(maxHealth * hpPercent);
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }
}