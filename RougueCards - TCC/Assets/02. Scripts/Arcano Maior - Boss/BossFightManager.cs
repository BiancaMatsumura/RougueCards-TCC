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
    [SerializeField] private BossIndicatorHUD bossHUD;

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
        bossHUD.Hide();
    }

    void Update()
    {
        if (_bossAlive) return;
        if (_currentBossIndex >= bosses.Count) return;
        if (_p1 == null || _p2 == null) return;

        var condition = bosses[_currentBossIndex];

        float xpProgress = condition.xpStageRequired > 0
    ? (float)playerProgress.currentStage / condition.xpStageRequired : -1f;
        float killsProgress = condition.killsRequired > 0
            ? (float)(_p1.kills + _p2.kills) / condition.killsRequired : -1f;
        float timeProgress = condition.timeRequired > 0
            ? Time.timeSinceLevelLoad / condition.timeRequired : -1f;

        // Média só dos requisitos ativos (valor != -1)
        float sum = 0f; int count = 0;
        if (xpProgress >= 0f) { sum += Mathf.Clamp01(xpProgress); count++; }
        if (killsProgress >= 0f) { sum += Mathf.Clamp01(killsProgress); count++; }
        if (timeProgress >= 0f) { sum += Mathf.Clamp01(timeProgress); count++; }

        float average = count > 0 ? sum / count : 1f;
        bossHUD.UpdateBar(average);

        bool xpOk = xpProgress < 0f || xpProgress >= 1f;
        bool killsOk = killsProgress < 0f || killsProgress >= 1f;
        bool timeOk = timeProgress < 0f || timeProgress >= 1f;

        if (xpOk && killsOk && timeOk)
        {
            bossHUD.Hide();
            TriggerBoss(condition);
        }
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
        bossHUD.Hide();
    }

    void OnPanelClosed()
    {
        if (!_bossAlive)
            spawner.enabled = true;
    }
}
