using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damage = 10;
    public float damageInterval = 1f; // tempo entre danos

    private float timer = 0f;

    private void OnTriggerStay(Collider other)
    {
        var health = other.GetComponent<Health>();

        if (health != null)
        {
            timer += Time.deltaTime;

            if (timer >= damageInterval)
            {
                health.TakeDamage(damage);
                timer = 0f;
                Debug.Log($"Dealt {damage} damage to Player {health.playerID}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        timer = 0f; // reset quando sair do trigger
    }
}