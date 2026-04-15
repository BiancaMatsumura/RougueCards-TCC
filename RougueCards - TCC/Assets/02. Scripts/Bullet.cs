using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 3f;

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}