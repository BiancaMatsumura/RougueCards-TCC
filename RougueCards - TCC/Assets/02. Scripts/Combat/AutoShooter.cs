using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [Header("Weapon Data")]
    public RangedWeaponData weaponData;

    [Header("Tiro")]
    [SerializeField] private Transform firePoint;

    private float timer;

    void Update()
    {
        if (weaponData == null) return;

        timer += Time.deltaTime;

        if (timer >= weaponData.fireRate)
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

    void Shoot(Transform target)
    {
        Vector3 baseDir = (target.position - firePoint.position).normalized;

        int pellets = Mathf.Max(1, weaponData.pellets);

        for (int i = 0; i < pellets; i++)
        {
            GameObject bullet = Instantiate(
                weaponData.bulletPrefab,
                firePoint.position,
                Quaternion.identity
            );

            Vector3 dir = ApplySpread(baseDir, weaponData.spread);

            Bullet b = bullet.GetComponent<Bullet>();
            b.Init(dir);
            b.SetDamage(weaponData.damage);
        }
    }

    Vector3 ApplySpread(Vector3 direction, float spread)
    {
        if (spread <= 0f) return direction;

        float angle = Random.Range(-spread, spread);
        return Quaternion.Euler(0, angle, 0) * direction;
    }
}