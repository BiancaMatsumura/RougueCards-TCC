using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador unificado do inimigo. 
/// Gerencia IA, Knockback e inicialização modular via EnemyData.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DamageDealer))]
public class Enemy : MonoBehaviour
{
    [Header("Configurações de Dados")]
    [SerializeField] private EnemyData data;
    [SerializeField] private float updateTargetRate = 0.5f; // Otimização: tempo para recalcular alvo

    [Header("Configurações de Knockback")]
    [SerializeField] private float knockbackResistance = 5f;

    [Header("Slider de Vida")]
    [SerializeField] private Slider slideLife;

    // Referências de Componentes
    private Health health;
    private DamageDealer damageDealer;
    private EnemyXP enemyXP;

    // Estado Interno
    private Transform target;
    private float targetTimer;
    private Vector3 knockbackVelocity;
    private bool isKnockedBack;
    private bool _initialized = false;

    void Awake()
    {
        health = GetComponent<Health>();
        damageDealer = GetComponent<DamageDealer>();
        enemyXP = GetComponent<EnemyXP>();
    }

    void Start()
    {
        if (!_initialized && data != null)
            Initialize(data);
    }


    /// <summary>
    /// Configura o inimigo com base em um EnemyData. Permite reaproveitar o prefab.
    /// </summary>
    public void Initialize(EnemyData newData)
    {
        _initialized = true;
        data = newData;

        // 1. Configuração Visual
        if (data.visualPrefab != null)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Visual")) Destroy(child.gameObject);
            }

            GameObject model = Instantiate(data.visualPrefab, transform);
            model.transform.localScale = Vector3.one * data.modelScale;
            model.tag = "Visual";

            Animator anim = model.GetComponent<Animator>();
            if (anim != null && data.animatorController != null)
            {
                anim.runtimeAnimatorController = data.animatorController;
            }

            //  Busca o slider dentro do visualPrefab instanciado
            slideLife = model.GetComponentInChildren<Slider>(true);
        }

        // 2. Sincronização de Atributos
        health.maxHealth = data.maxHealth;
        health.currentHealth = data.maxHealth;
        health.ForceRefresh();
        damageDealer.damage = data.damageToPlayer;

        if (enemyXP != null) enemyXP.SetXPValue(data.xpValue);

        var hitFlash = GetComponent<HitFlash>();
        if (hitFlash != null)
            hitFlash.RefreshRenderers();

        //  Conecta o slider ao Health após tudo inicializado
        SetupHealthSlider();
    }

    private void SetupHealthSlider()
    {
        if (slideLife == null) return;

        slideLife.minValue = 0f;
        slideLife.maxValue = health.maxHealth;
        slideLife.value = health.currentHealth;

        health.OnHealthChanged += UpdateSlider;
    }

    private void UpdateSlider(int _, int current, int max)
    {
        if (slideLife == null) return;
        slideLife.maxValue = max; 
        slideLife.value = current;
    }

    void OnDestroy()
    {
        // Evita memory leak: remove listener ao destruir o inimigo
        if (health != null)
            health.OnHealthChanged -= UpdateSlider;
    }
    void Update()
    {
        HandleKnockback();
        HandleAI();
    }

    /// <summary>
    /// Processa a física do Knockback de forma independente do movimento.
    /// </summary>
    private void HandleKnockback()
    {
        if (knockbackVelocity.magnitude > 0.01f)
        {
            isKnockedBack = true;
            transform.position += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * knockbackResistance);
        }
        else
        {
            isKnockedBack = false;
        }
    }

    /// <summary>
    /// Gerencia busca de alvo e deslocamento.
    /// </summary>
    private void HandleAI()
    {
        if (data == null) return;
        // Otimização: Busca o jogador apenas em intervalos definidos
        targetTimer += Time.deltaTime;
        if (targetTimer >= updateTargetRate)
        {
            FindClosestPlayer();
            targetTimer = 0f;
        }

        // Se estiver sob efeito de knockback ou sem alvo, não persegue
        if (isKnockedBack || target == null) return;

        // Movimentação
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * data.speed * Time.deltaTime;

        // Rotação suave para a direção do movimento
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// Aplica força de repulsão externa.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction * force;
    }

    /// <summary>
    /// Busca o objeto com tag "Player" mais próximo.
    /// </summary>
    private void FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            if (!p.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }
        target = closest;
    }
}