using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 3f;

    private Transform target;

    void Update()
    {
        if (target == null)
        {
            FindClosestPlayer();
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    void FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = Mathf.Infinity;

        foreach (var p in players)
        {
            if (!p.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                target = p.transform;
            }
        }
    }
}