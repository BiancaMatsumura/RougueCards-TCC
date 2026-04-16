using UnityEngine;

public class EnemyXP : MonoBehaviour
{
    [SerializeField]private GameObject XPprefab;

    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDeath += GiveXP;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDeath -= GiveXP;
    }

    void GiveXP()
    {
        if (XPprefab != null)
        {
            Instantiate(XPprefab, transform.position, Quaternion.identity);
        }

    }
}