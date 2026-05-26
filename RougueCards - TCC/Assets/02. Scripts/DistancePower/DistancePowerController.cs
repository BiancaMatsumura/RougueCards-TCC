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
    [Tooltip("Velocidade de preenchimento da barra durante a recarga (units/s)")]
    public float rechargeSpeed = 50f;

    [Tooltip("Porcentagem mínima que o power precisa cair antes de poder recarregar (0-1)")]
    public float rechargeThreshold = 0.5f; // 50% = precisa cair até metade
    private bool _canRecharge = false; // começa false para não ativar logo de início

    public float PowerP1 => _powerP1;
    public float PowerP2 => _powerP2;
    public bool IsBoostActiveP1 { get; private set; }
    public bool IsBoostActiveP2 { get; private set; }
    public bool IsRecharging { get; private set; }

    private VisualElement _fillP1;
    private VisualElement _fillP2;
    private VisualElement _powerContainerP1;
    private VisualElement _powerContainerP2;
    private const float BAR_HEIGHT = 522f;

    private float _powerP1;
    private float _powerP2;

    private bool _togetherLastFrame;
    private Coroutine _rechargeCoroutine;

    [Header("Poder Especial")]
    public PowerWave powerWave;
    public event System.Action<Vector3> OnPowerActivated;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _fillP1 = root.Q<VisualElement>("Fill_PowerP1");
        _fillP2 = root.Q<VisualElement>("Fill_PowerP2");
        _powerContainerP1 = root.Q<VisualElement>("PowerContainer01");
        _powerContainerP2 = root.Q<VisualElement>("PowerContainer02");

        _powerP1 = maxPower;
        _powerP2 = maxPower;

        OnPowerActivated += powerWave.Activate;
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

        // Verifica se baixou o suficiente para liberar recarga
        if (!_canRecharge && _powerP1 <= maxPower * (1f - rechargeThreshold))
            _canRecharge = true;

        // Inicia recarga ao se juntar, só se liberado
        if (together && !_togetherLastFrame && _rechargeCoroutine == null && _canRecharge)
            _rechargeCoroutine = StartCoroutine(RechargeRoutine());

        _togetherLastFrame = together;

        // ── Atualiza UI ──────────────────────────────────────────────────────
        ApplyBar(_fillP1, _powerP1);
        ApplyBar(_fillP2, _powerP2);
    }


    /// <summary>
    /// Retorna o multiplicador de velocidade que deve ser aplicado ao player (1 = normal, >1 = boost).
    /// Chame isso no PlayerController de cada player.
    /// </summary>
    public float GetSpeedMultiplier(int playerID)
    {
        bool boosted = playerID == 1 ? IsBoostActiveP1 : IsBoostActiveP2;
        return boosted ? boostMultiplier : 1f;
    }


    private IEnumerator RechargeRoutine()
    {
        IsRecharging = true;

        while (_powerP1 < maxPower || _powerP2 < maxPower)
        {
            var (p1, p2) = splitManager.Players;
            if (p1 == null || p2 == null || Vector3.Distance(p1.position, p2.position) >= splitManager.splitDistance)
            {
                IsRecharging = false;
                _rechargeCoroutine = null;
                yield break;
            }

            float gain = rechargeSpeed * Time.deltaTime;
            _powerP1 = Mathf.Min(maxPower, _powerP1 + gain);
            _powerP2 = Mathf.Min(maxPower, _powerP2 + gain);

            yield return null;
        }

        _powerP1 = maxPower;
        _powerP2 = maxPower;

        ApplyBar(_fillP1, _powerP1);
        ApplyBar(_fillP2, _powerP2);

        IsRecharging = false;
        _rechargeCoroutine = null;

        StartCoroutine(PowerRoutine());
    }

    private IEnumerator PowerRoutine()
    {
        float pulseDuration = 0.4f;

        for (int i = 0; i < 3; i++)
        {
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                float smooth = Mathf.SmoothStep(0f, 1f, elapsed / pulseDuration);
                float scale = Mathf.Lerp(1f, 1.1f, smooth);
                _powerContainerP1.style.scale = new Scale(new Vector3(scale, scale, 1f));
                _powerContainerP2.style.scale = new Scale(new Vector3(scale, scale, 1f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                float smooth = Mathf.SmoothStep(0f, 1f, elapsed / pulseDuration);
                float scale = Mathf.Lerp(1.1f, 1f, smooth);
                _powerContainerP1.style.scale = new Scale(new Vector3(scale, scale, 1f));
                _powerContainerP2.style.scale = new Scale(new Vector3(scale, scale, 1f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.15f);
        }

        _powerContainerP1.style.scale = new Scale(new Vector3(1f, 1f, 1f));
        _powerContainerP2.style.scale = new Scale(new Vector3(1f, 1f, 1f));

        var (p1, p2) = splitManager.Players;
        Vector3 center = (p1.position + p2.position) / 2f;
        OnPowerActivated?.Invoke(center);
        _canRecharge = false; // precisa drenar de novo antes do próximo uso
    }

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