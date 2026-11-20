using System.Collections;
using UnityEngine;

public class ShieldFeedback : MonoBehaviour
{
    [SerializeField] private PlayerBuffs buffs;
    [SerializeField] private GameObject bubble;        // esfera filha do jogador
    [SerializeField] private Renderer bubbleRenderer;  // material em modo Transparent
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip blockSound;
    [SerializeField] private AudioClip breakSound;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = bubble.transform.localScale;
        bubble.SetActive(false);
    }

    void OnEnable()
    {
        buffs.OnShieldChanged += HandleChanged;
        buffs.OnShieldBlocked += HandleBlocked;
    }

    void OnDisable()
    {
        buffs.OnShieldChanged -= HandleChanged;
        buffs.OnShieldBlocked -= HandleBlocked;
    }

    private void HandleChanged(int charges)
{
    bubble.SetActive(charges > 0);

    var c = bubbleRenderer.material.color;
    c.a = Mathf.Lerp(0.15f, 0.45f, charges / (float)Mathf.Max(1, buffs.ShieldMaxCharges));
    bubbleRenderer.material.color = c;

    if (charges == 0 && breakSound != null)
        audioSource.PlayOneShot(breakSound);
}

    private void HandleBlocked()
    {
        if (blockSound != null) audioSource.PlayOneShot(blockSound);

        FindFirstObjectByType<PopusManager>()
            ?.POPUPMensage(transform.position, "BLOQUEADO!", 2);

        StopAllCoroutines();
        StartCoroutine(Pulse());
    }

    private IEnumerator Pulse()
    {
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            bubble.transform.localScale = baseScale * (1f + 0.25f * Mathf.Sin(t / 0.15f * Mathf.PI));
            yield return null;
        }
        bubble.transform.localScale = baseScale;
    }
}