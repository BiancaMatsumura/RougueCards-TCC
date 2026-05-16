using System;
using System.Collections;
using UnityEngine;

public class DownedState : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float downedDuration = 10f;   // tempo até game over
    [SerializeField] private float reviveDuration = 3f;    // tempo para reviver
    [SerializeField] private float reviveHPPercent = 0.5f;
    public bool IsDowned { get; private set; } = false;

    public event Action OnDowned;
    public event Action OnRevived;
    public event Action OnDownedExpired;               // timer esgotou
    public event Action<float> OnDownedTimerChanged;   // 0..1 para a UI
    public event Action<float> OnReviveProgressChanged; // 0..1 para a barra

    private Coroutine _downedCoroutine;
    private Coroutine _reviveCoroutine;
    private DownedState _reviverState; // quem está me revivendo

    public void EnterDownedState()
    {
        Debug.Log($"[Downed] {name} entrou em estado abatido");
        if (IsDowned) return;

        IsDowned = true;
        OnDowned?.Invoke();
        _downedCoroutine = StartCoroutine(DownedTimerRoutine());
    }

    public void BeginRevive(DownedState reviver)
    {
        Debug.Log($"[Downed] {name} começando a ser revivido");
        if (!IsDowned || _reviveCoroutine != null) return;
        _reviverState = reviver;
        _reviveCoroutine = StartCoroutine(ReviveRoutine());
    }

    public void InterruptRevive()
    {
        Debug.Log($"[Downed] {name} revive interrompido");
        if (_reviveCoroutine == null) return;
        StopCoroutine(_reviveCoroutine);
        _reviveCoroutine = null;
        _reviverState = null;
        OnReviveProgressChanged?.Invoke(0f);
    }


    private IEnumerator DownedTimerRoutine()
    {
        float elapsed = 0f;

        while (elapsed < downedDuration)
        {
            elapsed += Time.deltaTime;
            OnDownedTimerChanged?.Invoke(1f - elapsed / downedDuration);
            yield return null;
        }

        Debug.Log($"[Downed] {name} timer esgotou sem ser revivido");
        IsDowned = false;
        OnDownedExpired?.Invoke();
    }

    private IEnumerator ReviveRoutine()
    {
        float elapsed = 0f;

        while (elapsed < reviveDuration)
        {
            elapsed += Time.deltaTime;
            Debug.Log($"[Downed] Progresso revive {name}: {elapsed / reviveDuration:P0}");
            OnReviveProgressChanged?.Invoke(elapsed / reviveDuration);
            yield return null;
        }

        Debug.Log($"[Downed] {name} revivido com sucesso!");
        // Revive concluído
        if (_downedCoroutine != null)
        {
            StopCoroutine(_downedCoroutine);
            _downedCoroutine = null;
        }

        IsDowned = false;
        _reviveCoroutine = null;

        var health = GetComponent<Health>();
        if (health != null) health.Revive(reviveHPPercent);

        OnRevived?.Invoke();
        OnReviveProgressChanged?.Invoke(0f);
    }
}