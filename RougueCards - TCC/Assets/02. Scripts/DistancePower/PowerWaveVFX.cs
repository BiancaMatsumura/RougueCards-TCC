using System.Collections;
using UnityEngine;

public class PowerWaveVFX : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float maxScale;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public void Play(Vector3 center, float radius)
    {
        maxScale = radius * 2f;
        transform.position = center;
        gameObject.SetActive(true);
        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            float scale = scaleCurve.Evaluate(t) * maxScale;
            transform.localScale = new Vector3(scale, 0.1f, scale);

            float alpha = alphaCurve.Evaluate(t);
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Alpha", alpha);
            _renderer.SetPropertyBlock(_mpb);

            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}