using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Área de Spawn")]
    [SerializeField] private Vector3 areaSize = new Vector3(10f, 0f, 10f);

    [Header("Controle")]
    [SerializeField] private float spawnDelay = 3f;
    [SerializeField] private int maxEnemies = 5;

    private List<GameObject> currentEnemies = new List<GameObject>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // remove inimigos destruídos
        currentEnemies.RemoveAll(e => e == null);

        if (timer >= spawnDelay && currentEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPos = GetRandomPointInArea();

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemies.Add(enemy);
    }

    Vector3 GetRandomPointInArea()
    {
        Vector3 center = transform.position;

        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float z = Random.Range(-areaSize.z / 2f, areaSize.z / 2f);

        return new Vector3(center.x + x, center.y, center.z + z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}