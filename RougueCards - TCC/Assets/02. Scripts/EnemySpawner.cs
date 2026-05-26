using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gerencia a criação de inimigos em uma área específica.
/// Alteração: Suporta múltiplos prefabs base e sorteio aleatório de tipos de inimigos.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Configuração de Prefabs")]
    /// <summary> Lista de prefabs base que podem ser instanciados (Inimigo, Boss, etc). </summary>
    [SerializeField] private GameObject[] enemyBasePrefabs;

    /// <summary> Lista de configurações (DNA) que podem ser aplicadas aos prefabs. </summary>
    [SerializeField] private EnemyData[] availableEnemyData;

    [Header("Área de Spawn")]
    [SerializeField] private Vector3 areaSize = new Vector3(10f, 0f, 10f);

    [Header("Controle de Quantidade")]
    [SerializeField] private float spawnDelay = 3f;
    [SerializeField] private int maxEnemies = 5;

    /// <summary> Lista interna para rastrear inimigos vivos. </summary>
    private List<GameObject> currentEnemies = new List<GameObject>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // Alteração: Limpa a lista removendo objetos destruídos ou desativados
        currentEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        if (timer >= spawnDelay && currentEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    /// <summary>
    /// Instancia um inimigo aleatório e aplica uma configuração de dados aleatória.
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyBasePrefabs.Length == 0 || availableEnemyData.Length == 0) return;

        // Sorteia um Prefab e um Dado
        GameObject randomPrefab = enemyBasePrefabs[Random.Range(0, enemyBasePrefabs.Length)];
        EnemyData randomData = availableEnemyData[Random.Range(0, availableEnemyData.Length)];

        Vector3 spawnPos = GetRandomPointInArea();
        GameObject enemyObj = Instantiate(randomPrefab, spawnPos, Quaternion.identity);

        // Inicializa o inimigo com os dados sorteados
        Enemy enemyScript = enemyObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(randomData);
        }

        currentEnemies.Add(enemyObj);
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