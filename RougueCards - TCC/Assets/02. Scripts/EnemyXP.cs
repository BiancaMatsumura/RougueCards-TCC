using UnityEngine;

public class EnemyXP : MonoBehaviour
{
    [SerializeField] private int xpValue = 1;
    [SerializeField] private PlayerProgress playerProgress;

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
        if (playerProgress != null)
        {
            playerProgress.AddXP(xpValue);
        }
    }
}