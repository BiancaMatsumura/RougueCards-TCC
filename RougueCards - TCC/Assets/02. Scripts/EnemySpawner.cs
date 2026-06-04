using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawner estilo Vampire Survivors:
/// nova horda só começa quando todos os inimigos morrem.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs de Inimigos")]
    [SerializeField] public GameObject[] prefabsInimigos;
    [SerializeField] private EnemyData[] dadosInimigosDisponiveis;

    [Header("Jogadores")]
    [SerializeField] private Transform jogador1;
    [SerializeField] private Transform jogador2;

    [Header("Área de Spawn")]
    [SerializeField] private float raioSpawn = 15f;
    [SerializeField] private float distanciaMinimaJogador = 6f;

    [Header("Hordas")]
    [SerializeField] private int inimigosPorHorda = 10;
    [SerializeField] private float intervaloEntreSpawns = 0.1f;

    [Header("Referências")]
    public SplitScreenManager splitManager;

    private List<GameObject> inimigosAtivos = new List<GameObject>();

    private bool spawnNoJogador1 = true;
    public event System.Action OnHordaCompleted;

    void Start()
    {
        StartCoroutine(CicloDeHordas());
    }

    void Update()
    {
        if (jogador1 == null || jogador2 == null)
        {
            var (p1, p2) = splitManager.Players;
            jogador1 = p1;
            jogador2 = p2;
        }

        inimigosAtivos.RemoveAll(i => i == null || !i.activeInHierarchy);
    }

    IEnumerator CicloDeHordas()
    {
        while (true)
        {
            yield return new WaitUntil(() => inimigosAtivos.Count == 0);
            OnHordaCompleted?.Invoke(); // avisa que a horda terminou
            yield return StartCoroutine(SpawnHorda(inimigosPorHorda));
        }
    }

    IEnumerator SpawnHorda(int quantidade)
    {
        for (int i = 0; i < quantidade; i++)
        {
            SpawnInimigo();
            yield return new WaitForSeconds(intervaloEntreSpawns);
        }
    }

    void SpawnInimigo()
    {
        if (prefabsInimigos.Length == 0 || dadosInimigosDisponiveis.Length == 0)
            return;

        GameObject prefab =
            prefabsInimigos[Random.Range(0, prefabsInimigos.Length)];

        EnemyData dados =
            dadosInimigosDisponiveis[Random.Range(0, dadosInimigosDisponiveis.Length)];

        Transform jogadorAlvo =
            spawnNoJogador1 ? jogador1 : jogador2;

        spawnNoJogador1 = !spawnNoJogador1;

        if (jogadorAlvo == null)
            return;

        Vector3 posicaoSpawn =
            ObterPosicaoAleatoriaProxima(jogadorAlvo);

        GameObject inimigo =
            Instantiate(prefab, posicaoSpawn, Quaternion.identity);

        Enemy scriptInimigo =
            inimigo.GetComponent<Enemy>();

        if (scriptInimigo != null)
        {
            scriptInimigo.Initialize(dados);
        }

        inimigosAtivos.Add(inimigo);
    }

    public Vector3 ObterPosicaoAleatoriaProxima(Transform jogador)
    {
        Vector3 direcaoAleatoria =
            Random.insideUnitSphere * raioSpawn;

        direcaoAleatoria.y = 0f;

        Vector3 posicao =
            jogador.position + direcaoAleatoria;

        Vector3 direcaoNormalizada =
            (posicao - jogador.position).normalized;

        return jogador.position +
               direcaoNormalizada * distanciaMinimaJogador;
    }
}