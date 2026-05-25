using RougueCards.Attributes;
using System;
using UnityEngine;

/// <summary>
/// Gerencia a vitalidade de jogadores e inimigos.
/// Alteração: O PlayerStats tornou-se opcional. Se presente (Player), usa lógica de atributos.
/// Se ausente (Inimigo), usa apenas os valores base definidos no script ou EnemyData.
/// </summary>
public class Health : MonoBehaviour
{
    public int playerID;

    public int maxHealth = 100;
    public int currentHealth;

    public event Action<int, int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnHit;

    private PlayerStats pStats;
    private Animation anim;
    private bool _isDead = false;

    void Awake()
    {
        // Alteração: Tenta pegar o componente, mas não o exige mais via [RequireComponent]
        pStats = GetComponent<PlayerStats>();

        GameOverWatcher.Instance?.RegisterPlayer(this);
        anim = GetComponent<Animation>();

        if (currentHealth <= 0) currentHealth = maxHealth;
    }

    void Start()
    {
        // Alteração: Só sincroniza MaxHP se for um objeto com PlayerStats (Jogador)
        if (pStats != null)
        {
            RefreshMaxHP();
        }

        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }

    /// <summary>
    /// Processa o dano recebido.
    /// Alteração: Evasão e Armadura agora só são calculadas se o objeto for um Jogador (tiver PlayerStats).
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        // 1. Lógica exclusiva de Jogador
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

        if (GetComponent<Enemy>() != null && anim != null)
        {
            anim.Play("DAMAGEVFX");
        }

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

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

    /// <summary>
    /// Atualiza a vida máxima baseada nos atributos.
    /// Alteração: Adicionado filtro para logar apenas se for um Jogador real (ID > 0).
    /// </summary>
    public void RefreshMaxHP()
    {
        if (pStats != null)
        {
            int oldMax = maxHealth;
            maxHealth = (int)pStats.stats.MaxHP.Value;

            if (maxHealth > oldMax && oldMax != 0)
            {
                currentHealth += (maxHealth - oldMax);
            }

            OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);

            // Só loga no console se for o Player 1 ou Player 2
            if (playerID > 0)
            {
                Debug.Log($"[Health] Player {playerID} MaxHP sincronizado: {maxHealth}");
            }
        }
    }

    public void Revive(float hpPercent)
    {
        _isDead = false;
        currentHealth = (int)(maxHealth * hpPercent);
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }
}
