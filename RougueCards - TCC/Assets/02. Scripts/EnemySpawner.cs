using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

/// <summary>
/// Spawner estilo Vampire Survivors:
/// nova horda só começa quando todos os inimigos morrem.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs de Inimigos")]
    
    [SerializeField] public EnemyWaveConfig[] Waves;
   
    public int hordaAtual = 0;
    public Transform ancorcaos;

    [Header("Jogadores")]
    [SerializeField] public Transform jogador1;
    [SerializeField] public Transform jogador2;

    [Header("Área de Spawn")]
    [SerializeField] private float raioSpawn = 15f;
    [SerializeField] private float distanciaMinimaJogador = 6f;

    [Header("Hordas")]
    [SerializeField] private int inimigosPorHorda = 10;
    [SerializeField] private float intervaloEntreSpawns = 0.1f;
    [SerializeField] private float tempoMaximoHorda = 30f;

    [Header("Referências")]
    public SplitScreenManager splitManager;
    [SerializeField] private EnterPlayersUI enterPlayersUI;

    private List<GameObject> inimigosAtivos = new List<GameObject>();

    public bool spawnNoJogador1 = true;
    public event System.Action OnHordaCompleted;



    void Start()
    {   
        enabled = false;
        enterPlayersUI.OnSelectionComplete += StartSpawner;
        CheckSpecialEnemySetup();
    }

    /// <summary>
    /// Avisa no Console se a lista de inimigos atrapalha a escolha exclusiva de carta:
    /// todos especiais (quem escolhe passa a mudar a cada abate, parecendo aleatório) ou nenhum especial
    /// (quem escolhe a carta é quem tem mais combo). O ideal é marcar só 1 tipo de inimigo como especial.
    /// </summary>
    private void CheckSpecialEnemySetup()
    {
        if (Waves[0].dadosInimigosDisponiveis == null)
        {
            return;
        }

        int total = 0;
        int special = 0;

        foreach (EnemyData data in Waves[0].dadosInimigosDisponiveis)
        {
            if (data == null)
            {
                continue;
            }

            total++;

            if (data.isSpecialEnemy)
            {
                special++;
            }
        }

        if (total < 2)
        {
            return;
        }

        if (special == total)
        {
            Debug.LogWarning("[Inimigo Especial] TODOS os inimigos da lista do EnemySpawner estão marcados como 'Is Special Enemy'. " +
                             "Assim, quem escolhe a carta muda a cada abate e parece aleatório. " +
                             "Deixe marcado só 1 tipo de inimigo (abra o EnemyData e desmarque nos outros).");
        }
        else if (special == 0)
        {
            Debug.Log("[Inimigo Especial] Nenhum inimigo da lista está marcado como 'Is Special Enemy'. " +
                      "Por enquanto quem escolhe a carta é o jogador com mais combo. " +
                      "Para usar a escolha por inimigo especial, marque 'Is Special Enemy' em 1 EnemyData.");
        }
    }
    private void StartSpawner()
    {
        enterPlayersUI.OnSelectionComplete -= StartSpawner;
        enabled = true;
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
           
            float tempo = 0f;
            
            while (inimigosAtivos.Count > 0 && tempo < tempoMaximoHorda)
            {
                tempo += Time.deltaTime;
                yield return null;
            }

    
            StopTraps();
            InitializeCaoticEvents();
            Debug.LogWarning($"Onda atual é {hordaAtual}");
            

            OnHordaCompleted?.Invoke();

            yield return StartCoroutine(SpawnHorda(inimigosPorHorda,hordaAtual));
        }
    }

    IEnumerator SpawnHorda(int quantidade, int waveactual)
    {
        for (int i = 0; i < quantidade; i++)
        {
            SpawnInimigo(waveactual);
            yield return new WaitForSeconds(intervaloEntreSpawns);
        }
    }

    void SpawnInimigo(int waveactual)
    {
        if (Waves[waveactual].prefabsInimigos.Length == 0 || Waves[waveactual].dadosInimigosDisponiveis.Length == 0)
            return;
        GameObject prefab =
           Waves[waveactual].prefabsInimigos[Random.Range(0, Waves[waveactual].prefabsInimigos.Length)];
        EnemyData dados =
            Waves[waveactual].dadosInimigosDisponiveis[Random.Range(0, Waves[waveactual].dadosInimigosDisponiveis.Length)];
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
    public IEnumerator NextWave()
    {  
        if(Waves.Length < hordaAtual)
        {
            Debug.LogError("Horda máxima atingida");
            yield return 0;
        }
        else
        {
            hordaAtual += 1 ;
            StopAllCoroutines();
            inimigosAtivos.Clear();
            Debug.LogError("Corrtinas paradas");
            StartCoroutine(CicloDeHordas());
            Debug.LogError("Corrtinas iniciadas");
            
        }
    }
    public void InitializeCaoticEvents()
    {
        if(Waves[hordaAtual].HasCaoticEvents)
        {
              Debug.LogError("temos caos");
            foreach(var caos in Waves[hordaAtual].CaoticGameObjects)
            {
                if(caos != null){
                     var obj = Instantiate(caos,ancorcaos); 
                     obj.GetComponent<CaoticEvents>().SpawnCenter = ancorcaos;
                     obj.GetComponent<CaoticEvents>().Reward(); 
                     Debug.LogError("temos caoitc gameobjecrs");
                    }
                
            }
        }
        else
        {
             StopTraps();
        }
        }

    public void StopTraps()
    {
        GameObject[] a = GameObject.FindGameObjectsWithTag("caoticevents");
             foreach(var caos in a)
            {
                caos.GetComponent<CaoticEvents>().StopSpawn();
            }
    }
}