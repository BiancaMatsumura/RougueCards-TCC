using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float updateTargetRate = 0.5f; // tempo pra trocar alvo

    private Transform target;
    private float timer;

    void Update()
    {
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
}