using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DistancePowerController : MonoBehaviour
{
    [Header("Referências")]
    public SplitScreenManager splitManager;

    [Header("Configuração de Power")]
    [Tooltip("Velocidade de perda de power quando afastados (units/s)")]
    public float powerLossSpeed = 20f;

    [Tooltip("Power máximo de cada player")]
    public int maxPower = 100;

    [Header("Configuração de Boost")]
    [Tooltip("Multiplicador de velocidade do boost quando apontando um pro outro")]
    public float boostMultiplier = 2f;

    [Tooltip("Ângulo máximo (graus) para considerar que estão apontando um pro outro")]
    public float facingAngleThreshold = 45f;

    [Header("Configuração de Recarga")]
    [Tooltip("Tempo (s) que os players precisam ficar juntos antes de ativar o poder")]
    public float rechargeTime = 2f;

    [Tooltip("Velocidade de preenchimento da barra durante a recarga (units/s)")]
    public float rechargeSpeed = 50f;

    // ── eventos ──────────────────────────────────────────────────────────────
    /// <summary>Disparado quando a recarga completa. Assine para ativar efeitos especiais.</summary>
    public event System.Action OnPowerActivated;

    // ── estado público (leitura) ──────────────────────────────────────────────
    public float PowerP1 => _powerP1;
    public float PowerP2 => _powerP2;
    public bool IsBoostActiveP1 { get; private set; }
    public bool IsBoostActiveP2 { get; private set; }
    public bool IsRecharging { get; private set; }

    // ── privados ─────────────────────────────────────────────────────────────
    private VisualElement _fillP1;
    private VisualElement _fillP2;
    private const float BAR_HEIGHT = 522f;

    private float _powerP1;
    private float _powerP2;

    private bool _togetherLastFrame;
    private Coroutine _rechargeCoroutine;

    // ── Unity ────────────────────────────────────────────────────────────────
    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _fillP1 = root.Q<VisualElement>("Fill_PowerP1");
        _fillP2 = root.Q<VisualElement>("Fill_PowerP2");

        _powerP1 = maxPower;
        _powerP2 = maxPower;
    }

    void Update()
    {
        var (p1, p2) = splitManager.Players;
        if (p1 == null || p2 == null) return;

        float dist = Vector3.Distance(p1.position, p2.position);
        bool together = dist < splitManager.splitDistance;

        // ── Lógica de power ──────────────────────────────────────────────────
        if (!together)
        {
            // Afastados: drena o poder de ambos
            float loss = powerLossSpeed * Time.deltaTime;
            _powerP1 = Mathf.Max(0f, _powerP1 - loss);
            _powerP2 = Mathf.Max(0f, _powerP2 - loss);

            // Cancela recarga se saíram enquanto recarregavam
            if (_rechargeCoroutine != null)
            {
                StopCoroutine(_rechargeCoroutine);
                _rechargeCoroutine = null;
                IsRecharging = false;
            }
        }

        // ── Lógica de boost (só enquanto tiver poder) ────────────────────────
        bool hasPowerP1 = _powerP1 > 0f;
        bool hasPowerP2 = _powerP2 > 0f;

        IsBoostActiveP1 = !together && hasPowerP1 && IsFacingTarget(p1, p2);
        IsBoostActiveP2 = !together && hasPowerP2 && IsFacingTarget(p2, p1);

        // ── Inicia recarga ao se juntar ──────────────────────────────────────
        if (together && !_togetherLastFrame && _rechargeCoroutine == null)
            _rechargeCoroutine = StartCoroutine(RechargeRoutine());

        _togetherLastFrame = together;

        // ── Atualiza UI ──────────────────────────────────────────────────────
        ApplyBar(_fillP1, _powerP1);
        ApplyBar(_fillP2, _powerP2);
    }

    // ── Boost ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna o multiplicador de velocidade que deve ser aplicado ao player (1 = normal, >1 = boost).
    /// Chame isso no PlayerController de cada player.
    /// </summary>
    public float GetSpeedMultiplier(int playerID)
    {
        bool boosted = playerID == 1 ? IsBoostActiveP1 : IsBoostActiveP2;
        return boosted ? boostMultiplier : 1f;
    }

    // ── Recarga ──────────────────────────────────────────────────────────────

    private IEnumerator RechargeRoutine()
    {
        IsRecharging = true;
        float elapsed = 0f;

        while (elapsed < rechargeTime)
        {
            // Se se afastarem, cancela (o Update já para a coroutine, mas por segurança)
            var (p1, p2) = splitManager.Players;
            if (p1 == null || p2 == null || Vector3.Distance(p1.position, p2.position) >= splitManager.splitDistance)
            {
                IsRecharging = false;
                _rechargeCoroutine = null;
                yield break;
            }

            // Enche o poder gradualmente
            float gain = rechargeSpeed * Time.deltaTime;
            _powerP1 = Mathf.Min(maxPower, _powerP1 + gain);
            _powerP2 = Mathf.Min(maxPower, _powerP2 + gain);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Garante que chegou ao máximo
        _powerP1 = maxPower;
        _powerP2 = maxPower;

        IsRecharging = false;
        _rechargeCoroutine = null;

        // Dispara o poder especial
        OnPowerActivated?.Invoke();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Verifica se 'from' está com o forward apontando para 'to' dentro do threshold.</summary>
    private bool IsFacingTarget(Transform from, Transform to)
    {
        Vector3 dir = (to.position - from.position).normalized;
        float dot = Vector3.Dot(from.forward, dir);
        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
        return angle <= facingAngleThreshold;
    }

    private void ApplyBar(VisualElement fill, float current)
    {
        float percent = current / maxPower;
        fill.style.height = percent * BAR_HEIGHT;
    }

    // ── Compatibilidade com chamadas externas existentes ─────────────────────
    public void UpdatePower(int playerID, int current, int max)
    {
        float percent = (float)current / max;
        if (playerID == 1) _fillP1.style.height = percent * BAR_HEIGHT;
        if (playerID == 2) _fillP2.style.height = percent * BAR_HEIGHT;
    }
}