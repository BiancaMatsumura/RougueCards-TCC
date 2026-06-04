using RougueCards.Attributes;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public int damage = 20;
    [SerializeField] private PlayerStats pStats;

    void Awake()
    {
        pStats = GetComponentInParent<PlayerStats>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage, pStats);
        }
    }
}
