using UnityEngine;

/// <summary>
/// Aplica dano contínuo a objetos com o componente Health dentro de um trigger.
/// Alteração: Adicionado filtro de Tag para evitar que inimigos causem dano uns aos outros.
/// </summary>
public class DamageDealer : MonoBehaviour
{
    public int damage = 10;
    public float damageInterval = 1f;

    private float timer = 0f;

    private void OnTriggerStay(Collider other)
    {
        // RÍGIDO: Só causa dano se o objeto atingido tiver EXATAMENTE a tag Player
        if (other.CompareTag("Player"))
        {
            if (other.CompareTag(gameObject.tag)) return; // mesma tag = ignora

            var health = other.GetComponent<Health>();
            if (health != null)
            {
                timer += Time.deltaTime;
                if (timer >= damageInterval)
                {
                    health.TakeDamage(damage);
                    timer = 0f;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer = 0f;
        }
    }
}
