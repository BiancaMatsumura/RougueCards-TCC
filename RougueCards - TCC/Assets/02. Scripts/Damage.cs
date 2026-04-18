using UnityEngine;

public class Damage : MonoBehaviour
{
    public int damage = 20;
  void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

        }
    }
}
