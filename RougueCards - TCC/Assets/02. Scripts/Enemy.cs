using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador unificado do inimigo. 
/// Gerencia IA, Knockback e inicialização modular via EnemyData.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DamageDealer))]
[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour
{
    [Header("Configurações de Dados")]
    [SerializeField] private EnemyData data;

    [SerializeField] private float updateTargetRate = 0.5f;

    [Header("Configurações de Knockback")]
    [SerializeField] private float knockbackResistance = 5f;

    [Header("Slider de Vida")]
    [SerializeField] private Slider slideLife;

    private Health health;
    private DamageDealer damageDealer;
    private EnemyXP enemyXP;
    private AudioSource audioSource;

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
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.OnHit += HandleHitSound;
            health.OnDeath += HandleDeath;
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.OnHit -= HandleHitSound;
            health.OnDeath -= HandleDeath;
        }
    }

    void Start()
    {
        if (!_initialized && data != null)
            Initialize(data);
    }

    public void Initialize(EnemyData newData)
    {
        _initialized = true;
        data = newData;

        if (data.visualPrefab != null)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Visual"))
                    Destroy(child.gameObject);
            }

            GameObject model = Instantiate(data.visualPrefab, transform);
            model.transform.localScale = Vector3.one * data.modelScale;
            model.tag = "Visual";

            Animator anim = model.GetComponent<Animator>();
            if (anim != null && data.animatorController != null)
                anim.runtimeAnimatorController = data.animatorController;

            slideLife = model.GetComponentInChildren<Slider>(true);
        }

        health.maxHealth = data.maxHealth;
        health.currentHealth = data.maxHealth;
        health.ForceRefresh();

        damageDealer.damage = data.damageToPlayer;

        if (enemyXP != null)
            enemyXP.SetXPValue(data.xpValue);

        SetupHealthSlider();
    }

    private void SetupHealthSlider()
    {
        if (slideLife == null) return;

        slideLife.minValue = 0;
        slideLife.maxValue = health.maxHealth;
        slideLife.value = health.currentHealth;

        health.OnHealthChanged += UpdateSlider;
    }

    private void UpdateSlider(int _, int current, int max)
    {
        if (slideLife != null)
            slideLife.value = current;
    }

    // SOM DE DANO
    private void HandleHitSound()
    {
        if (data == null || data.damageSound == null) return;

        audioSource.PlayOneShot(data.damageSound);
    }

    // MORTE (CORRIGIDO DEFINITIVAMENTE)
    private void HandleDeath()
    {
        if (data == null || data.deathSound == null) return;

        PlayDeathSoundSafe(data.deathSound);

        StartCoroutine(DeathRoutine());
    }

    private void PlayDeathSoundSafe(AudioClip clip)
    {
        GameObject tempAudio = new GameObject("DeathSound");
        AudioSource src = tempAudio.AddComponent<AudioSource>();

        src.clip = clip;


        src.Play();

        Destroy(tempAudio, clip.length + 0.1f);
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        yield return null;
        Destroy(gameObject);
    }

    void Update()
    {
      
        HandleAI();
    }

  

    private void HandleAI()
    {
        if (data == null) return;

        targetTimer += Time.deltaTime;

        if (targetTimer >= updateTargetRate)
        {
            FindClosestPlayer();
            targetTimer = 0;
        }

        if (isKnockedBack || target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * data.speed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction * force;
  
    }

    private void FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            if (!p.activeInHierarchy) continue;
            if (p.GetComponent<DownedState>()?.IsDowned == true) continue;

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