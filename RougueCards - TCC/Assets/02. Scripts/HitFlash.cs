using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float flashIntensity = 3f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();

        var health = GetComponent<Health>();
        if (health != null)
            health.OnHit += Flash;
    }

    void OnDestroy()
    {
        var health = GetComponent<Health>();
        if (health != null)
            health.OnHit -= Flash;
    }

    public void RefreshRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in _renderers)
        {
            if (r == null) continue;
            r.material.EnableKeyword("_EMISSION");
        }
    }

    private void Flash()
    {
        if (_renderers == null || _renderers.Length == 0)
            RefreshRenderers();

        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetEmission(flashColor * flashIntensity);
        yield return new WaitForSeconds(flashDuration);
        SetEmission(Color.black);
    }

    private void SetEmission(Color color)
    {
        foreach (var r in _renderers)
        {
            if (r == null) continue;
            _mpb = new MaterialPropertyBlock();
            _mpb.SetColor("_EmissionColor", color);
            r.SetPropertyBlock(_mpb);
        }
    }
}