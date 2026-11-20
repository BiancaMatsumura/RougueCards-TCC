using System;
using System.Collections.Generic;
using RougueCards.Attributes;
using UnityEngine;

public class PlayerBuffs : MonoBehaviour
{
    [SerializeField] private float shieldBlockWindow = 0.5f;

    private float shareUntil;
    private float shareRadius;
    private float oneHitKillUntil;
    private int shieldCharges;
    private float nextShieldBlockTime;

    public event Action<int> OnShieldChanged;   // cargas restantes
    public event Action OnShieldBlocked;        // disparou ao absorver um golpe

    public bool HasOneHitKill => Time.time < oneHitKillUntil;
    public bool HasDamageShare => Time.time < shareUntil;
    public int ShieldCharges => shieldCharges;

    private int shieldMaxCharges;
    public int ShieldMaxCharges => shieldMaxCharges;

    public void AddShield(int charges)
    {
        shieldCharges += charges;
        shieldMaxCharges = shieldCharges;
        OnShieldChanged?.Invoke(shieldCharges);
    }

    public void ActivateDamageShare(float duration, float radius)
    {
        shareUntil = Time.time + duration;
        shareRadius = radius;
    }

    public void ActivateOneHitKill(float duration)
    {
        oneHitKillUntil = Time.time + duration;
    }


    /// Retorna true se o escudo absorveu o golpe.
    public bool TryBlockWithShield()
    {
        if (shieldCharges <= 0) return false;

        // Janela curta: evita gastar 3 cargas em 3 frames de dano por contato
        if (Time.time < nextShieldBlockTime) return true;

        shieldCharges--;
        nextShieldBlockTime = Time.time + shieldBlockWindow;
        OnShieldChanged?.Invoke(shieldCharges);
        OnShieldBlocked?.Invoke();
        return true;
    }

    /// Redireciona o dano só para os inimigos próximos. Retorna true se redirecionou (jogador não leva nada).
    public bool TryRedirectDamage(int damage, PlayerStats source)
    {
        if (!HasDamageShare) return false;

        var targets = new List<Health>();
        float sqrRadius = shareRadius * shareRadius;

        foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if ((e.transform.position - transform.position).sqrMagnitude > sqrRadius) continue;
            if (e.TryGetComponent(out Health h) && !h._isDead) targets.Add(h);
        }

        // Sem inimigos por perto, o jogador leva o dano normal
        if (targets.Count == 0) return false;

        int share = Mathf.Max(1, Mathf.CeilToInt(damage / (float)targets.Count));
        foreach (var h in targets)
        {
            h.TakeDamage(share, source);
        }

        return true;
    }
}