using UnityEngine;
using RougueCards.Attributes;

public class AutoShooter : MonoBehaviour
{
    [Header("Weapon Data")]
    public RangedWeaponData weaponData; //seria interessante depois transfomrar em array


    [Header("Tiro")]
    [SerializeField] private Transform firePoint;

    private float timer;
    private PlayerStats pStats;

    void Start()
    {
        // Busca os atributos no jogador (geralmente no mesmo objeto ou no pai)
        pStats = GetComponentInParent<PlayerStats>();
    }

    void Update()
    {
        if (weaponData == null) return;

        // --- CÁLCULO DE ATRIBUTO ---
        // Ajusta a cadência de tiro com base no atributo AttackSpeed (ex: 100% = normal)
        float attackSpeedMod = pStats != null ? (pStats.stats.AttackSpeed.Value / 100f) : 1f;
        float adjustedFireRate = weaponData.fireRate / attackSpeedMod;

        timer += Time.deltaTime;

        if (timer >= adjustedFireRate)
        {
            Transform target = FindClosestEnemy();

            if (target != null)
            {
                Shoot(target);
                timer = 0f;
            }
        }
      
    }

    // 🔥 ADICIONADO (CORREÇÃO DO ERRO)
    public void SetWeapon(RangedWeaponData newWeapon)
    {
        weaponData = newWeapon;
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (dist < minDist && dist <= weaponData.range)
            {
                minDist = dist;
                closest = enemy.transform;
            }
        }

        return closest;
    }


    /// <summary>
    /// Calcula e instancia os projéteis aplicando bônus de quantidade, dano, velocidade e duração.
    /// </summary>
    void Shoot(Transform target)
    {
        if (firePoint == null || weaponData == null) return;

        Vector3 baseDir = (target.position - firePoint.position).normalized;

        int extraPellets = pStats != null ? (int)pStats.stats.ProjectileQty.Value : 0;
        int totalPellets = Mathf.Max(1, weaponData.pellets + extraPellets);

        // 1. Cálculo de Dano Base
        float baseDamage = weaponData.damage + (pStats != null ? pStats.stats.Damage.Value : 0);

        float chance = pStats != null ? pStats.stats.CritChance.Value : 0f;
        float multiplier = pStats != null ? pStats.stats.CritDamage.Value : 1f;

        // Sorteio: se o número (0-100) for menor que a chance, é um Crítico!
        bool isCrit = UnityEngine.Random.Range(0f, 100f) < chance;
        float finalDamage = isCrit ? baseDamage * (1f + multiplier) : baseDamage;

        float finalKnockback = pStats != null ? pStats.stats.Knockback.Value : 0f;

        if (isCrit)
        {
            Debug.Log($"[AutoShooter] ACERTO CRÍTICO! Dano: {finalDamage}");
        }

        float finalProjSpeed = pStats != null ? pStats.stats.ProjectileSpeed.Value : 10f;

        float durationMod = pStats != null ? pStats.stats.Duration.Value : 1f;
        float finalLifeTime = weaponData.lifetime * durationMod;

        for (int i = 0; i < totalPellets; i++)
        {
            GameObject bulletObj = Instantiate(weaponData.bulletPrefab, firePoint.position, Quaternion.identity);
            Vector3 dir = ApplySpread(baseDir, weaponData.spread);

            Bullet b = bulletObj.GetComponent<Bullet>();
            if (b != null)
            {
                // Passamos o dano (que pode ser crítico ou não) para a bala
                b.Init(dir, (int)finalDamage, finalProjSpeed, finalLifeTime, finalKnockback, weaponData.DestroyOnContact); //aaaaaaaaaaaaaa
                b.ApplyMovimentType(weaponData.BM, this);
    {

    }
}
        }
    }

    Vector3 ApplySpread(Vector3 direction, float spread)
    {
        if (spread <= 0f) return direction;

        float angle = Random.Range(-spread, spread);
        return Quaternion.Euler(0, angle, 0) * direction;
    }

    public Vector3 GetTrasform() 
    { 
        return this.transform.position;
    }
}
