using UnityEngine;

public class EnemyXP : MonoBehaviour
{
    [SerializeField] private GameObject XPprefab;
    [SerializeField] private GameObject healthPickupPrefab;

    [Range(0f, 1f)]
    [SerializeField] private float xpChance = 0.5f; // 50% XP, 50% vida

    private int _xpValue;
    private Health health;

    void Awake() => health = GetComponent<Health>();

    public void SetXPValue(int val) => _xpValue = val;

    void OnEnable()
    {
        if (health != null)
        { health.OnDeath += GiveDrop; }
    }
    void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath -= GiveDrop;
        }
    }

    void GiveDrop()
    {
        // Sorteia entre 0 e 1: se for menor que xpChance, dropa XP; senão, dropa vida
        GameObject prefabToSpawn = Random.value < xpChance ? XPprefab : healthPickupPrefab;

        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        }
    }
}