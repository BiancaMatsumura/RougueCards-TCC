using UnityEngine;
using RougueCards.Attributes;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 10;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float updateTargetRate = 0.2f;

    private bool isAttracted = false;
    private Transform target;
    private float timer;

    void Update()
    {
        // Ainda não está sendo atraída: verifica se algum jogador está no alcance
        if (!isAttracted)
        {
            CheckForNearbyPlayers();
            return;
        }

        timer += Time.deltaTime;

        // Atualiza o alvo periodicamente para seguir o jogador mais próximo
        if (timer >= updateTargetRate)
        {
            FindClosestPlayer();
            timer = 0f;
        }

        // Ninguém precisa de vida agora: volta a ficar parada
        if (target == null)
        {
            isAttracted = false;
            return;
        }

        // Movimento em direção ao jogador (efeito magnético)
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    /// <summary>
    /// Ativa o magnetismo quando um jogador que precisa de cura entra no
    /// alcance de coleta (CollectionRange).
    /// </summary>
    private void CheckForNearbyPlayers()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var p in players)
        {
            if (!CanReceiveHeal(GetHealth(p))) continue;

            PlayerStats stats = p.GetComponent<PlayerStats>();
            if (stats == null) continue;

            float range = stats.stats.CollectionRange.Value;
            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist <= range)
            {
                isAttracted = true;
                target = p.transform;
                break;
            }
        }
    }

    /// <summary>
    /// Escolhe o jogador mais próximo que ainda pode receber cura.
    /// </summary>
    private void FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            if (!p.activeInHierarchy) continue;
            if (!CanReceiveHeal(GetHealth(p))) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }

        target = closest; // null se ninguém precisar de cura
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Health playerHealth = GetHealth(other.gameObject);
        if (!CanReceiveHeal(playerHealth)) return;

        playerHealth.Heal(healAmount);
        Destroy(gameObject);
    }

    private Health GetHealth(GameObject player)
    {
        Health h = player.GetComponent<Health>();
        if (h == null) h = player.GetComponentInChildren<Health>();
        if (h == null) h = player.GetComponentInParent<Health>();
        return h;
    }

    private bool CanReceiveHeal(Health h)
    {
        return h != null && !h._isDead && h.currentHealth < h.maxHealth;
    }
}