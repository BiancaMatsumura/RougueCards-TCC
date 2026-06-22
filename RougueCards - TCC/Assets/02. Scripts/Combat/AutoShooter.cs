using System.Collections.Generic;
using UnityEngine;
using RougueCards.Attributes;

public class AutoShooter : MonoBehaviour
{
    [Header("Armas Equipadas")]
    [SerializeField] private List<RangedWeaponData> weapons = new List<RangedWeaponData>();

    [Header("Tiro")]
    [SerializeField] private Transform firePoint;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private PlayerStats pStats;
    private float[] timers;

    private Vector3 recoilOffset;

    void Awake()
    {
        pStats = GetComponentInParent<PlayerStats>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        timers = new float[weapons.Count];
    }

    void Update()
    {
        if (weapons.Count == 0) return;

        if (timers == null || timers.Length != weapons.Count)
            timers = new float[weapons.Count];

        for (int i = 0; i < weapons.Count; i++)
        {
            var weapon = weapons[i];
            if (weapon == null) continue;

            float attackSpeedMod =
                pStats != null ? (pStats.stats.AttackSpeed.Value / 100f) : 1f;

            float fireRate = weapon.fireRate / attackSpeedMod;

            timers[i] += Time.deltaTime;

            if (timers[i] >= fireRate)
            {
                Transform target = FindClosestEnemy(weapon);

                if (target != null)
                {
                    Shoot(weapon, target);
                    ApplyRecoil(weapon, target);
                    PlayShootSound(weapon);
                    timers[i] = 0f;
                }
            }
        }
    }

    void LateUpdate()
    {
        transform.position += recoilOffset;
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, 10f * Time.deltaTime);
    }

    public void AddWeapon(RangedWeaponData newWeapon)
    {
        if (newWeapon == null) return;

        weapons.Add(newWeapon);
        timers = new float[weapons.Count];

        Debug.Log("Arma adicionada: " + newWeapon.name);
    }

    Transform FindClosestEnemy(RangedWeaponData weapon)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var e in enemies)
        {
            if (!e.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);

            if (dist > weapon.range) continue;

            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }

        return closest;
    }

    void Shoot(RangedWeaponData weapon, Transform target)
    {
        if (firePoint == null || weapon == null || weapon.bulletPrefab == null) return;

        Vector3 baseDir = (target.position - firePoint.position).normalized;

        int pellets = Mathf.Max(1, weapon.pellets);

        for (int i = 0; i < pellets; i++)
        {
            GameObject bulletObj =
                Instantiate(weapon.bulletPrefab, firePoint.position, Quaternion.identity);

            Vector3 dir = baseDir;

            if (weapon.spread > 0f)
            {
                float angle = Random.Range(-weapon.spread, weapon.spread);
                dir = Quaternion.Euler(0, angle, 0) * baseDir;
            }

            Bullet b = bulletObj.GetComponent<Bullet>();

            if (b != null)
            {
                b.Init(
                    dir,
                    weapon.damage,
                    weapon.lifetime,
                    0f,
                    weapon.DestroyOnContact,
                    pStats
                );

                b.ApplyMovimentType(weapon.BM, this);
            }
        }
    }

    void PlayShootSound(RangedWeaponData weapon)
    {
        if (audioSource == null) return;

        if (weapon.shootSound != null)
        {
            audioSource.PlayOneShot(weapon.shootSound);
        }
    }

    void ApplyRecoil(RangedWeaponData weapon, Transform target)
    {
        if (weapon.recoilForce <= 0f) return;

        Vector3 dir = (target.position - firePoint.position).normalized;
        recoilOffset += -dir * weapon.recoilForce;
    }

    public Vector3 GetTrasform()
    {
        return transform.position;
    }
}