using UnityEngine;
using RougueCards.Attributes;

public class XP : MonoBehaviour
{
    [SerializeField] private PlayerProgress playerProgress;
    [SerializeField] private int xpValue = 1;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float updateTargetRate = 0.2f;

    public bool isAttracted = false;

    private Transform target;
    private float timer;

    void Update()
    {
        // Se ainda não estiver sendo atraído, verifica se há algum jogador por perto
        if (!isAttracted)
        {
            CheckForNearbyPlayers();
            return;
        }

        timer += Time.deltaTime;

        // Atualiza a posição do alvo periodicamente para seguir o jogador em movimento
        if (timer >= updateTargetRate)
        {
            FindClosestPlayer();
            timer = 0f;
        }

        if (target == null) return;

        // Move o XP em direção ao jogador (Efeito Magnético)
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    /// <summary>
    /// Varre todos os jogadores na cena e verifica se este XP está dentro do alcance 
    /// de coleta definido pelo atributo CollectionRange na StatSheet de cada jogador.
    /// Se estiver no alcance, ativa o magnetismo para o jogador mais próximo.
    /// </summary>
    private void CheckForNearbyPlayers()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var p in players)
        {
            PlayerStats stats = p.GetComponent<PlayerStats>();
            if (stats == null) continue;

            // Obtém o valor real do atributo CollectionRange (Base + Upgrades)
            float range = stats.stats.CollectionRange.Value;

            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist <= range)
            {
                isAttracted = true;
                target = p.transform;
                break; // Encontrou um jogador, pode começar a perseguição
            }
        }
    }

    void FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            if (!p.activeInHierarchy)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, p.transform.position);

            // Se esta distância for a menor encontrada até agora, este é o novo alvo em potencial
            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }

        // Só atualiza o alvo se encontrarmos alguém
        if (closest != null)
        {
            target = closest;
        }
    }

    /// <summary>
    /// Detecta o contato com o jogador e entrega o XP multiplicado.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            // Pega o multiplicador (ex: 1.0 = 100%, 1.5 = 150%)
            float multiplier = stats != null ? stats.stats.XPMultiplier.Value : 1f;

            // Calcula o valor final (arredondado para inteiro)
            int finalXP = Mathf.RoundToInt(xpValue * multiplier);

            if (playerProgress != null)
            {
                playerProgress.AddXP(finalXP);
                Debug.Log($"XP Coletado: {finalXP} (Base: {xpValue} x Bônus: {multiplier})");
            }

            Destroy(gameObject);
        }
    }
}
