using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float updateTargetRate = 0.5f; // tempo pra trocar alvo

    private Transform target;
    private float timer;

    // Variáveis de Knockback
    private Vector3 knockbackVelocity;
    [SerializeField] private float knockbackResistance = 5f; // Quão rápido ele para de deslizar
    private bool _isKnockedBack;

    void Update()
    {
        if (knockbackVelocity.magnitude > 0.01f)
        {
            _isKnockedBack = true;
            transform.position += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * knockbackResistance);
        }
        else
        {
            _isKnockedBack = false;
        }

        timer += Time.deltaTime;
        if (timer >= updateTargetRate)
        {
            FindClosestPlayer();
            timer = 0f;
        }

        if (target == null || _isKnockedBack) return; // para a perseguição durante knockback

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    /// <summary>
    /// Aplica uma força de repulsão instantânea ao inimigo.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        // Define a velocidade de empurrão baseada na direção da bala e na força do atributo
        knockbackVelocity = direction * force;
    }

    void FindClosestPlayer()
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