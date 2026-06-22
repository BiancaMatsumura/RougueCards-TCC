using UnityEngine;
using RougueCards.Attributes;

public enum BulletMoviment
{
    Linear,
    Curve,
    Orbital,
    Static,
    Follow,
    Acelerating,
    RandomFall,
    Boomerang,
    FollowTarget,
    Eletric,
}

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float knockbackForce;

    private PlayerStats shooter;

    private Vector3 direction;
    Vector3 playertrasform;

    BulletMoviment bm = BulletMoviment.Linear;

    private bool DestroyOC;
    private AutoShooter autoshooterscript;
    private bool IsSpawned = true;

    public void Init(Vector3 dir, int dmg, float lTime, float kb, bool DestroyOnC, PlayerStats owner = null)
    {
        direction = dir;
        damage = dmg;
        lifeTime = lTime;
        knockbackForce = kb;
        DestroyOC = DestroyOnC;
        shooter = owner;

        Destroy(gameObject, lifeTime);
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void Update()
    {
        if (autoshooterscript != null)
        {
            Moviment(bm);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Ground"))
        {
            var health = other.GetComponent<Health>();

            if (health != null)
            {
                health.pStats = shooter;
                health.TakeDamage(damage, shooter);
            }

            if (DestroyOC || other.CompareTag("Ground"))
            {
                Destroy(gameObject);
            }
        }
    }

    public void ApplyMovimentType(BulletMoviment BM, AutoShooter autoshooter)
    {
        bm = BM;
        autoshooterscript = autoshooter;
        playertrasform = autoshooter.GetTrasform();
    }

    public void Moviment(BulletMoviment BM)
    {
        switch (BM)
        {
            case BulletMoviment.Linear:
                transform.position += direction * speed * Time.deltaTime;
                break;

            case BulletMoviment.Curve:
                Vector3 curvePos = Vector3.Lerp(transform.position, direction, 0.1f);
                transform.position = curvePos * speed;
                break;

            case BulletMoviment.Orbital:
                transform.position = GetOrbitalPosition(
                    autoshooterscript.GetTrasform(),
                    5f
                );
                break;

            case BulletMoviment.Static:
                break;

            case BulletMoviment.Follow:
                Vector3 followDir =
                    (autoshooterscript.GetTrasform() - transform.position).normalized;

                transform.position += followDir * 10f * Time.deltaTime;
                break;

            case BulletMoviment.Acelerating:
                float acceleration = 1f;
                speed += acceleration * Time.deltaTime;

                Vector3 accelDir =
                    (autoshooterscript.GetTrasform() - transform.position).normalized;

                transform.position += accelDir * speed * Time.deltaTime;
                break;

            case BulletMoviment.RandomFall:
                if (IsSpawned)
                {
                    this.transform.position =
                        GetRandomPositionInCircle(playertrasform, 5f) + Vector3.up * 50f;
                }

                IsSpawned = false;

                transform.position += Vector3.down * 10f * Time.deltaTime;
                break;

            case BulletMoviment.Boomerang:
                Vector3 targetPos =
                    autoshooterscript.GetTrasform() + direction * 5f;

                float t = Mathf.PingPong(Time.time * speed, 1f);

                transform.position = Vector3.Lerp(
                    autoshooterscript.GetTrasform(),
                    targetPos,
                    t
                );
                break;

            case BulletMoviment.FollowTarget:
                break;

            case BulletMoviment.Eletric:
                break;
        }
    }

    Vector3 GetOrbitalPosition(Vector3 center, float radius)
    {
        float angle = Time.time * speed;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        return center + new Vector3(x, 0f, z);
    }

    public Vector3 GetRandomPositionInCircle(Vector3 center, float radius)
    {
        Vector2 random = Random.insideUnitCircle * radius;
        return center + new Vector3(random.x, 1f, random.y);
    }

    public Vector3 LinearMoviment()
    {
        return direction * speed * Time.deltaTime;
    }
}