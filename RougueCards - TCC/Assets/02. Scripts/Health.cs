using RougueCards.Attributes;
using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
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
    void Start()
    {
        // Alteração: Agora busca o valor inicial do MaxHP nos atributos em vez de usar fixo
        RefreshMaxHP();

        // Inicia o jogo com a vida cheia
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }

    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);

        pStats = GetComponent<PlayerStats>();
        GameOverWatcher.Instance?.RegisterPlayer(this);
        anim = GetComponent<Animation>();
    }

    /// <summary>
    /// Processa o dano recebido.
    /// Primeiro verifica a chance de Evasão (esquiva total).
    /// Se falhar, aplica a redução por Armadura e subtrai da vida.
    /// </summary>
    public void TakeDamage(int amount)
    {
        // 1. Verificação de Evasão (Atributo dinâmico)
        if (pStats != null)
        {
            float evasionChance = pStats.stats.Evasion.Value;

            if (UnityEngine.Random.Range(0f, 100f) < evasionChance)
            {
                Debug.Log($"[Health] Player {playerID} ESQUIVOU do dano! (Chance: {evasionChance}%)");
                return;
            }
        }

        int finalDamage = amount;

        // 2. Verificação de Armadura (Atributo dinâmico)
        if (pStats != null)
        {
            float armorValue = pStats.stats.Armor.Value;
            finalDamage = Mathf.Max(1, amount - (int)armorValue);
        }

        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
        OnHit?.Invoke();

        if (GetComponent<Enemy>() != null && anim != null)
        {
            anim.Play("DAMAGEVFX");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return;   // <- guard
        _isDead = true;

        var downed = GetComponent<DownedState>();
        if (downed != null)
        {
            // Game over só dispara se o timer expirar sem ser revivido
            downed.OnDownedExpired += () =>
            {
                OnDeath?.Invoke();
                gameObject.SetActive(false);
            };
            downed.EnterDownedState();
        }
        else
        {
            // Sem DownedState no objeto (ex: inimigo) → morte direta como antes
            OnDeath?.Invoke();
            gameObject.SetActive(false);
        }
    }
    public void Revive(float hpPercent)
    {
        _isDead = false;
        currentHealth = (int)(maxHealth * hpPercent);
        OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
    }

    /// <summary>
    /// Sincroniza a vida máxima com a ficha de atributos (PlayerStats).
    /// Chamado pelo AttributeMaestro sempre que uma carta de MaxHP é coletada.
    /// </summary>
    public void RefreshMaxHP()
    {
        if (pStats != null)
        {
            int oldMax = maxHealth;

            // Atualiza o valor de maxHealth baseado no cálculo final da StatSheet (Base + Bônus)
            maxHealth = (int)pStats.stats.MaxHP.Value;

            // Lógica de Cura: Se a vida máxima aumentou, o jogador é curado pela diferença
            if (maxHealth > oldMax && oldMax != 0)
            {
                currentHealth += (maxHealth - oldMax);
            }

            // Notifica a UI sobre a mudança na barra de vida
            OnHealthChanged?.Invoke(playerID, currentHealth, maxHealth);
            Debug.Log($"Player {playerID} MaxHP atualizado para: {maxHealth}");
        }
    }

}