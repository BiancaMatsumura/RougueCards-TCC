using System.Collections.Generic;
using UnityEngine;

public class PowerWave : MonoBehaviour
{
    [Header("Configuração da Onda")]
    public float radius = 5f;
    public int damage = 30;
    public float knockbackForce = 10f;
    public LayerMask enemyLayer;
    [Header("VFX")]
    public PowerWaveVFX waveVFX;

    public void Activate(Vector3 center)
    {
        if (waveVFX != null)
            waveVFX.Play(center, radius);
        

        Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer);
        

        var alreadyHit = new HashSet<GameObject>();

        foreach (var hit in hits)
        {
            GameObject root = hit.transform.root.gameObject;
            if (!alreadyHit.Add(root)) continue;

            if (!root.CompareTag("Enemy")) continue;

            var health = hit.GetComponent<Health>() ?? hit.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                
            }
            Vector3 dir = (root.transform.position - center).normalized;
            var enemy = root.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ApplyKnockback(dir, knockbackForce);
            }
        }
    }
}