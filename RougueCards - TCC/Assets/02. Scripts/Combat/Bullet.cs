using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float knockbackForce;

    private Vector3 direction;

    /// <summary>
    /// Configura as propriedades da bala no momento do disparo.
    /// </summary>
    /// <param name="dir">Direção do movimento.</param>
    /// <param name="dmg">Dano causado ao impacto.</param>
    /// <param name="spd">Velocidade de deslocamento.</param>
    /// <param name="lTime">Tempo em segundos antes de se auto-destruir.</param>
    public void Init(Vector3 dir, int dmg, float spd, float lTime, float kb)
    {
        direction = dir;
        damage = dmg;
        speed = spd;
        lifeTime = lTime;
        knockbackForce = kb;

        Destroy(gameObject, lifeTime);
    }

    // Mantido por compatibilidade, mas o dano já é definido no Init
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void Update()
    {
        // Agora utiliza a variável speed que foi definida no Init
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