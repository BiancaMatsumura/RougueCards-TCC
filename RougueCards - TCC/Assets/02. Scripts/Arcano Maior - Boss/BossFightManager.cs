using System.Collections.Generic;
using UnityEngine;
using RougueCards.Attributes;

public class BossFightManager : MonoBehaviour
{
    [Header("Bosses por estágio")]
    public List<ArcanoMaior> bosses;

    [Header("Referências")]
    public PlayerProgress playerProgress;
    public CardManager cardManager;

    private PlayerStats _p1;
    private PlayerStats _p2;
    private int _currentBossIndex = 0;
    private bool _bossAlive = false;

    [SerializeField] private EnemySpawner spawner;

    void Start()
    {
        playerProgress.bossAlive = false;
        cardManager.OnPanelClosed += OnPanelClosed;

        var splitScreen = FindFirstObjectByType<SplitScreenManager>();
        splitScreen.OnPlayersRegistered += (p1, p2) =>
        {
            _p1 = p1.GetComponent<PlayerStats>();
            _p2 = p2.GetComponent<PlayerStats>();
        };
    }

    void Update()
    {
        if (_bossAlive) return;
        if (_currentBossIndex >= bosses.Count) return;
        if (_p1 == null || _p2 == null) return;

        var condition = bosses[_currentBossIndex];
        bool xpOk = playerProgress.currentStage >= condition.xpStageRequired;
        bool killsOk = (_p1.kills + _p2.kills) >= condition.killsRequired;
        bool timeOk = Time.timeSinceLevelLoad >= condition.timeRequired;

        if (xpOk && killsOk && timeOk)
            TriggerBoss(condition);
    }

    void TriggerBoss(ArcanoMaior condition)
    {
        _bossAlive = true;
        playerProgress.bossAlive = true;
        spawner.enabled = false;

        Vector3 pos = spawner.ObterPosicaoAleatoriaProxima(_p1.transform);
        GameObject bossObj = Instantiate(spawner.prefabsInimigos[0], pos, Quaternion.identity);

        var enemy = bossObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(condition.bossEnemyData);

            var hp = bossObj.GetComponent<Health>();
        }

        var health = bossObj.GetComponent<Health>();
        if (health != null) health.OnDeath += OnBossDied;
    }

    void OnBossDied()
    {
        _bossAlive = false;
        playerProgress.bossAlive = false;
        _currentBossIndex++;
        playerProgress.CompleteStage();
    }

    void OnPanelClosed()
    {
        if (!_bossAlive)
            spawner.enabled = true;
    }
}
