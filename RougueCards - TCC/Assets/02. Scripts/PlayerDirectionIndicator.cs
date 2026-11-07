using UnityEngine;

public class PlayerDirectionIndicator : MonoBehaviour
{
    [SerializeField] private SplitScreenManager splitManager;

    [Header("Seta (sprite apontando para CIMA no arquivo)")]
    [SerializeField] private SpriteRenderer arrowPrefab;
    [SerializeField] private float radius = 0.9f;
    [SerializeField] private float groundOffsetY = 0.05f;
    [SerializeField] private float arrowScale = 1f;

    [Header("Layers (para cada câmera ver só a própria seta)")]
    [SerializeField] private string layerP1 = "ArrowP1";
    [SerializeField] private string layerP2 = "ArrowP2";

    [Header("Cor por proximidade")]
    [SerializeField] private float nearDistance = 3f; 
    [SerializeField] private float farDistance = 15f;
    [SerializeField] private float colorSmoothing = 8f;

    private readonly Color _p1Base = new Color(0.05f, 0.07f, 0.15f);
    private readonly Color _p1Bright = new Color(0.2f, 0.3f, 1f);
    private readonly Color _p2Base = new Color(0.16f, 0.04f, 0.12f);
    private readonly Color _p2Bright = new Color(1f, 0.2f, 0.5f);

    private SpriteRenderer _arrowP1, _arrowP2;
    private Transform _p1, _p2;
    private float _closeness; // 0 = longe, 1 = perto

    void Awake()
    {
        _arrowP1 = Instantiate(arrowPrefab);
        _arrowP2 = Instantiate(arrowPrefab);

        _arrowP1.transform.localScale = Vector3.one * arrowScale;
        _arrowP2.transform.localScale = Vector3.one * arrowScale;
        _arrowP1.gameObject.layer = LayerMask.NameToLayer(layerP1);
        _arrowP2.gameObject.layer = LayerMask.NameToLayer(layerP2);
        _arrowP1.gameObject.SetActive(false);
        _arrowP2.gameObject.SetActive(false);
    }

    void OnEnable() => splitManager.OnSplitScreenChanged += OnSplitChanged;
    void OnDisable() => splitManager.OnSplitScreenChanged -= OnSplitChanged;

    void OnSplitChanged(bool isSplit)
    {
        if (isSplit)
        {
            (_p1, _p2) = splitManager.Players;
            _closeness = 0f;
        }

        _arrowP1.gameObject.SetActive(isSplit);
        _arrowP2.gameObject.SetActive(isSplit);
    }

    void LateUpdate()
    {
        if (_p1 == null || _p2 == null) return;

        PlaceArrow(_arrowP1, _p1, _p2);
        PlaceArrow(_arrowP2, _p2, _p1);

        // Distância no plano (ignora altura)
        Vector3 diff = _p2.position - _p1.position;
        diff.y = 0f;
        float distance = diff.magnitude;

        // 1 quando perto (<= nearDistance), 0 quando longe (>= farDistance)
        float target = 1f - Mathf.InverseLerp(nearDistance, farDistance, distance);

        // Suaviza para a cor não "pular"
        _closeness = Mathf.Lerp(_closeness, target, 1f - Mathf.Exp(-colorSmoothing * Time.deltaTime));

        _arrowP1.color = Color.Lerp(_p1Base, _p1Bright, _closeness);
        _arrowP2.color = Color.Lerp(_p2Base, _p2Bright, _closeness);
    }

    void PlaceArrow(SpriteRenderer arrow, Transform from, Transform to)
    {
        Vector3 dir = to.position - from.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        arrow.transform.position = from.position + dir * radius + Vector3.up * groundOffsetY;

        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
    }
}