using UnityEngine;
using UnityEngine.UI;

public class ReviveIndicatorCanvas : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Slider reviveSlider;

    private DownedState _downedState;
    private Camera _camera;

    void Awake()
    {
        _downedState = GetComponentInParent<DownedState>();
        _camera = Camera.main;

        _downedState.OnDowned += Show;
        _downedState.OnRevived += Hide;
        _downedState.OnDownedExpired += Hide;
        _downedState.OnReviveProgressChanged += UpdateBar;

        panel.SetActive(false);
    }

    void OnDestroy()
    {
        _downedState.OnDowned -= Show;
        _downedState.OnRevived -= Hide;
        _downedState.OnDownedExpired -= Hide;
        _downedState.OnReviveProgressChanged -= UpdateBar;
    }

    void LateUpdate()
    {
        if (!panel.activeSelf) return;

        // pega todas as câmeras ativas e escolhe a mais próxima
        Camera nearest = null;
        float minDist = float.MaxValue;

        foreach (var cam in Camera.allCameras)
        {
            float dist = Vector3.Distance(transform.position, cam.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = cam;
            }
        }

        if (nearest != null)
            transform.forward = nearest.transform.forward;
    }

    private void Show()
    {
        panel.SetActive(true);
        reviveSlider.value = 0f;
    }

    private void Hide() => panel.SetActive(false);

    private void UpdateBar(float progress) => reviveSlider.value = progress;
}