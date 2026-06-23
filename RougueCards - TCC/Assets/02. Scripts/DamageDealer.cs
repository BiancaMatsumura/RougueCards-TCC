using UnityEngine;

/// <summary>
/// Gerencia a aplicação de dano a alvos (geralmente o Player).
/// Garante que o primeiro dano seja instantâneo ao contato e os próximos sigam um intervalo.
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [Header("Configurações de Ataque")]
    [Tooltip("Quantidade de dano por golpe.")]
    public int damage = 10;

    [Tooltip("Tempo de espera entre um golpe e outro enquanto estiver encostado.")]
    public float damageInterval = 1f;

    // Controla o tempo do próximo ataque permitido
    private float nextDamageTime = 0f;
    
    private Rigidbody rb;
    void Start() => rb = GetComponent<Rigidbody>();

    /// <summary>
    /// Detecta objetos dentro do trigger. Se for o Player, aplica dano respeitando o cooldown.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // 1. Filtro de Tag: Só interage com o Player
        if (other.CompareTag("Player"))
        {
            // 2. Filtro de Segurança: Evita que o inimigo cause dano a si mesmo ou aliados com a mesma tag
            if (other.CompareTag(gameObject.tag)) return;

            if (rb != null && rb.IsSleeping()) rb.WakeUp();
            // 3. Lógica de Cooldown baseada no tempo global (mais precisa que timer acumulativo)
            if (Time.time >= nextDamageTime)
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    
                    // Define quando será o próximo ataque (Tempo Atual + Intervalo)
                    nextDamageTime = Time.time + damageInterval;
                    
                    Debug.Log($"Dano aplicado! Próximo ataque em: {damageInterval}s");
                }
            }
        }
    }

    /// <summary>
    /// Reseta o tempo de ataque quando o alvo sai do alcance, 
    /// garantindo que o próximo contato seja instantâneo novamente.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nextDamageTime = 0f;
        }
    }
}