using UnityEngine;

public class XP : MonoBehaviour
{

    [SerializeField] private PlayerProgress playerProgress;
    [SerializeField] private int xpValue = 1;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float updateTargetRate = 0.5f; 

    public bool isAttracted = false;

    private Transform target;
    private float timer;

    void Update()
    {
        if (!isAttracted) return;
        timer += Time.deltaTime;

        // Atualiza alvo a cada X tempo
        if (timer >= updateTargetRate)
        {
            FindClosestPlayer();
            timer = 0f;
        }

        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    void FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            if (!p.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }

        target = closest;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerProgress != null)
            {
                playerProgress.AddXP(xpValue);
                Debug.Log("Player collected XP!");
            }
            Destroy(gameObject);
        }
    }

}
