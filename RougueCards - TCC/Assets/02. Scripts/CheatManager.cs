using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Painel de cheats para testes rápidos durante o jogo.
/// Coloque em um GameObject vazio na cena de gameplay e associe o PlayerProgress no Inspector.
/// </summary>
public class CheatManager : MonoBehaviour
{
    [Header("Ativação")]
    [SerializeField] private bool cheatsEnabled = true;

    [Header("Referências")]
    [SerializeField] private PlayerProgress playerProgress;
    [SerializeField] private int xpPerCheat = 50;
    public EnemySpawner enemySpawner;
    public PowerWave pwwave;
    [Header("Teclas")]
    [SerializeField] private Key godModeKey = Key.F1;
    [SerializeField] private Key killAllEnemiesKey = Key.F2;
    [SerializeField] private Key healPlayersKey = Key.F3;
    [SerializeField] private Key addXPKey = Key.F4;
    [SerializeField] private Key completeStageKey = Key.F5;
    [SerializeField] private Key nextwave = Key.F6;
    [SerializeField] private Key nextwavenoclear = Key.F7;
    [SerializeField] private Key stopenemys = Key.F8;
    [SerializeField] private Key wavepower = Key.F9;

    private bool godMode = false;
    private bool isfreeze = false;
    void Update()
    {
        if (!cheatsEnabled)
        {
            return;
        }

        var kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        if (kb[godModeKey].wasPressedThisFrame)
        {
            ToggleGodMode();
        }
        if (kb[killAllEnemiesKey].wasPressedThisFrame)
        {
            KillAllEnemies();
        }
        if (kb[healPlayersKey].wasPressedThisFrame)
        {
            HealAllPlayers();
        }
        if (kb[addXPKey].wasPressedThisFrame)
        {
            AddXPToProgress();
        }
        if (kb[completeStageKey].wasPressedThisFrame)
        {
            CompleteStageInstantly();
        }
        if(kb[nextwave].wasPressedThisFrame)
        {
            NextWave();
        }
         if(kb[nextwavenoclear].wasPressedThisFrame)
        {
            NextWaveNoClear();
        }
           if(kb[stopenemys].wasPressedThisFrame)
        {
            FreezeAllEnemys();
        }
        if(kb[wavepower].wasPressedThisFrame)
        {
            WaveP();
        }
    }


    private void WaveP()
    {
    
        var pA = GameObject.FindGameObjectsWithTag("Player");
        
        if(pA[0] != null && pA[1] != null)
        {
            var p0 = pA[1].GetComponent<Transform>();
            var p1 = pA[0].GetComponent<Transform>();

           Vector3 media = (p0.position + p1.position) / 2f;
            media.y = 0;
            pwwave.Activate(media);
        }
        
   
    }
    private void NextWave()
    {
        KillAllEnemies();
        enemySpawner.NextWave();
        KillAllEnemies();
    }
    private void NextWaveNoClear()
    {
        enemySpawner.NextWave();
    }
    private void ToggleGodMode()
    {
        godMode = !godMode;

        foreach (var health in GetPlayersHealth())
        {
            health.isInvincible = godMode;
        }

        Debug.Log($"[Cheat] God mode: {(godMode ? "ATIVADO" : "DESATIVADO")}");
    }

    private void KillAllEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemyObj in enemies)
        {
            Destroy(enemyObj);
        }

        Debug.Log($"[Cheat] {enemies.Length} inimigo(s) eliminado(s).");
    }
    private void FreezeAllEnemys()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
       
        isfreeze = !isfreeze;

        foreach (var enemyObj in enemies)
        {
            var enemies2 = enemyObj.GetComponent<Enemy>();
            enemies2.SwichEnemyMove(isfreeze);
        }

        Debug.Log($"[Cheat] {enemies.Length} inimigo(s) eliminado(s).");
    }

    private void HealAllPlayers()
    {
        foreach (var health in GetPlayersHealth())
        {
            health.FullHeal();
        }

        Debug.Log("[Cheat] Jogadores curados.");
    }

    private void AddXPToProgress()
    {
        if (playerProgress == null)
        {
            Debug.LogWarning("[Cheat] PlayerProgress não atribuído no CheatManager.");
            return;
        }

        playerProgress.AddXP(xpPerCheat);
        Debug.Log($"[Cheat] +{xpPerCheat} XP concedido.");
    }

    private void CompleteStageInstantly()
    {
        if (playerProgress == null)
        {
            Debug.LogWarning("[Cheat] PlayerProgress não atribuído no CheatManager.");
            return;
        }

        playerProgress.CompleteStage();
        Debug.Log("[Cheat] Estágio completado.");
    }

    private System.Collections.Generic.IEnumerable<Health> GetPlayersHealth()
    {
        foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            var health = playerObj.GetComponent<Health>();
            if (health != null)
            {
                yield return health;
            }
        }
    }
}
