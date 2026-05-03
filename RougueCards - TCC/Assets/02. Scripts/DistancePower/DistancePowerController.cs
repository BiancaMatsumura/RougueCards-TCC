using UnityEngine;
using UnityEngine.UIElements;

public class DistancePowerController : MonoBehaviour
{
    [Header("Referências")]
    public SplitScreenManager splitManager;

    [Header("Configuração")]
    [Tooltip("Velocidade de subida/descida da barra (unidades de power por segundo)")]
    public float powerGainSpeed = 20f;   // velocidade ao ganhar (distantes)
    public float powerLossSpeed = 40f;   // velocidade ao perder (próximos)

    [Tooltip("Power máximo de cada player")]
    public int maxPower = 100;

    private VisualElement fillP1;
    private VisualElement fillP2;
    private const float BAR_HEIGHT = 522f;

    // power atual de cada player (float interno para suavidade)
    private float _powerP1;
    private float _powerP2;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        fillP1 = root.Q<VisualElement>("Fill_PowerP1");
        fillP2 = root.Q<VisualElement>("Fill_PowerP2");

        // começa no máximo
        _powerP1 = maxPower;
        _powerP2 = maxPower;
    }

    void Update()
    {
        var (p1, p2) = splitManager.Players;
        if (p1 == null || p2 == null) return;

        float dist = Vector3.Distance(p1.position, p2.position);

        // normaliza: 0 = juntos, 1 = no limite do split (ou além)
        float proximity = 1f - Mathf.Clamp01(dist / splitManager.splitDistance);
        // proximity=1 → juntos → perde power
        // proximity=0 → distantes → ganha power

        bool split = dist >= splitManager.splitDistance;
        float speed = split ? powerGainSpeed : -powerLossSpeed;
        float delta = speed * Time.deltaTime;

        _powerP1 = Mathf.Clamp(_powerP1 + delta, 0f, maxPower);
        _powerP2 = Mathf.Clamp(_powerP2 + delta, 0f, maxPower);

        ApplyBar(fillP1, _powerP1);
        ApplyBar(fillP2, _powerP2);
    }

    void ApplyBar(VisualElement fill, float current)
    {
        float percent = current / maxPower;
        fill.style.height = percent * BAR_HEIGHT;
    }

    // mantém compatibilidade com chamadas externas existentes
    public void UpdatePower(int playerID, int current, int max)
    {
        float percent = (float)current / max;
        if (playerID == 1) fillP1.style.height = percent * BAR_HEIGHT;
        if (playerID == 2) fillP2.style.height = percent * BAR_HEIGHT;
    }
}