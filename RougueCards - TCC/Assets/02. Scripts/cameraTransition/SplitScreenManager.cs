using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class SplitScreenManager : MonoBehaviour
{
    [Header("VCams")]
    public CinemachineCamera vcamGroup;
    public CinemachineCamera vcamP1;
    public CinemachineCamera vcamP2;

    [Header("Target Group")]
    public CinemachineTargetGroup targetGroup;

    [Header("Split Screen")]
    public Camera camP1;
    public Camera camP2;
    public RawImage rawImageP1;
    public RawImage rawImageP2;

    [Header("Configuração")]
    [Tooltip("Distância a partir da qual o split ativa")]
    public float splitDistance = 15f;
    [Tooltip("Suavidade da transição de UI (0=instantâneo, 1=lento)")]
    public float uiBlendSpeed = 2f;

    // prioridades
    private const int PRIORITY_HIGH = 15;
    private const int PRIORITY_LOW = 5;

    private Transform _p1;
    private Transform _p2;
    private bool _isSplit = false;

    // alpha atual dos painéis de split
    private float _splitAlpha = 0f;
    public bool isSplitScreen => _isSplit;
    public (Transform p1, Transform p2) Players => (_p1, _p2);
    public event System.Action<bool> OnSplitScreenChanged;

    void Start()
    {
        camP1.enabled = false;
        camP2.enabled = false;
    }

    void Update()
    {
        if (_p1 == null || _p2 == null) return;

        float dist = Vector3.Distance(_p1.position, _p2.position);
        bool shouldSplit = dist > splitDistance;

        if (shouldSplit != _isSplit)
        {
            _isSplit = shouldSplit;
            SetCameraMode(_isSplit);
        }

        // blend suave da UI de split
        float targetAlpha = _isSplit ? 1f : 0f;
        _splitAlpha = Mathf.Lerp(_splitAlpha, targetAlpha, Time.deltaTime * uiBlendSpeed);
        ApplySplitUI(_splitAlpha);
    }

    void SetCameraMode(bool split)
    {
        if (split)
        {
            vcamP1.Priority = PRIORITY_HIGH;
            vcamP2.Priority = PRIORITY_HIGH;
            vcamGroup.Priority = PRIORITY_LOW;
            camP1.enabled = true;
            camP2.enabled = true;
        }
        else
        {
            vcamGroup.Priority = PRIORITY_HIGH;
            vcamP1.Priority = PRIORITY_LOW;
            vcamP2.Priority = PRIORITY_LOW;
            camP1.enabled = false;
            camP2.enabled = false;
        }
        OnSplitScreenChanged?.Invoke(split);
    }

    void ApplySplitUI(float alpha)
    {
        // quando alpha=1: dois painéis lado a lado
        // quando alpha=0: painéis invisíveis, MainCamera ocupa tudo

        Color c1 = rawImageP1.color;
        Color c2 = rawImageP2.color;
        c1.a = alpha;
        c2.a = alpha;
        rawImageP1.color = c1;
        rawImageP2.color = c2;

        // esconde a MainCamera quando em split total
        // (opcional: depende do seu setup de Canvas)
    }

    // Chame isso do seu PlayerSpawner após instanciar os players
    public void RegisterPlayers(Transform p1, Transform p2)
    {
        _p1 = p1;
        _p2 = p2;

        // alimenta as VCams individuais
        vcamP1.Follow = p1;
        vcamP1.LookAt = p1;
        vcamP2.Follow = p2;
        vcamP2.LookAt = p2;

        // alimenta o TargetGroup
        targetGroup.Targets.Clear();
        targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = p1, Weight = 1f, Radius = 2f });
        targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = p2, Weight = 1f, Radius = 2f });
    }
}