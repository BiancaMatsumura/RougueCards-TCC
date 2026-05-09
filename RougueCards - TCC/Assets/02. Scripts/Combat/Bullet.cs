
using UnityEngine;


/// <summary>
/// Tipos de movimentos que uma projetil pode ter
/// </summary>
public enum BulletMoviment {
    Linear, // segue na direção que foi disparada
    Curve, // segue uma curva bezier
    Orbital, // gira em torno do player
    Static, // pode servir como armadilha
    Follow, //surge e segue o player. Útil para fazer bolhas de dano, lazers, e outros.
    Acelerating, // acelera conforme o tempo
    RandomFall, // surge do céu e cai em lugares aleatórios. 
    Boomerang, // vai e volta
    FollowTarget, //segue o inimigo mais proximo
    Eletric, //Especificamente para eletricidade, ele vai de um inimigo ao outro dando dano, gerando um raio entre eles. Correntes magicas é uma boa ideia tambem
    
}
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float knockbackForce;

    private Vector3 direction;
    Vector3 playertrasform;
    BulletMoviment bm = BulletMoviment.Linear;
    private bool DestroyOC;
    private AutoShooter autoshooterscript;
    private bool IsSpawned = true;

    /// <summary>
    /// Configura as propriedades da bala no momento do disparo.
    /// </summary>
    /// <param name="dir">Direção do movimento.</param>
    /// <param name="dmg">Dano causados ao impacto.</param>
    /// <param name="spd">Velocidade de deslocamento.</param>
    /// <param name="lTime">Tempo em segundos antes de se auto-destruir.</param>
    public void Init(Vector3 dir, int dmg, float spd, float lTime, float kb, bool DestroyOnC)
    {
        direction = dir;
        damage = dmg;
        speed = spd;
        lifeTime = lTime;
        knockbackForce = kb;

        DestroyOC = DestroyOnC;

        Destroy(gameObject, lifeTime);
       
    }

    // Mantido por compatibilidade, mas o dano já é definido no Init
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }


    void Update()
    {
        if(autoshooterscript != null) { Moviment(bm); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Ground"))
        {
            var health = other.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(damage);
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
        if (autoshooterscript = null)
            return;
        switch (BM) {

            case BulletMoviment.Linear:

                transform.position += direction * speed * Time.deltaTime;

                break;

            case BulletMoviment.Curve:

                Vector3 curvePos = Bezier(transform.position, direction, 10f);
                    

                transform.position = curvePos * speed;

                break;

            case BulletMoviment.Orbital:

                transform.position = GetOrbitalPosition(
                    autoshooterscript.GetTrasform(),
                    5f
                );

                break;

            case BulletMoviment.Static:

                Debug.Log("SemMovimento");

                break;

            case BulletMoviment.Follow:

                Vector3 followDir =
                    (autoshooterscript.GetTrasform() - transform.position).normalized;

                transform.position +=
                    followDir * 10f * Time.deltaTime;

                break;

            case BulletMoviment.Acelerating:

                float aceleration = 1f;

                speed += aceleration * Time.deltaTime;

                Vector3 accelDir =
                    (autoshooterscript.GetTrasform() - transform.position).normalized;

                transform.position +=
                    accelDir * speed * Time.deltaTime;

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

                float t =
                    Mathf.PingPong(Time.time * speed, 1f);

                transform.position = Vector3.Lerp(
                    autoshooterscript.GetTrasform(),
                    targetPos,
                    t
                );

                break;

            case BulletMoviment.FollowTarget:

                // segue o inimigo mais próximo

                break;

            case BulletMoviment.Eletric:

                // pula entre inimigos gerando correntes

                break;

       
        }
        
    }
    public static Vector3 Bezier(Vector3 start, Vector3 end, float t)
    {
        return Vector3.Lerp(start, end, t);
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
    public void GetPlayerTransform(Vector3 trasnformPosition) 
    {
        playertrasform = trasnformPosition;
    }
}