using RougueCards.Attributes;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Damage : MonoBehaviour
{
    public int damage = 20;
    [SerializeField] private PlayerStats pStats;
    public Collider principleCollider;
    [SerializeField] private float frametime = 0.2f;
    [SerializeField] private float KnockForceTest;

    private Shooter shooter;
    void Awake()
    {
        pStats = GetComponentInParent<PlayerStats>();
        
        shooter = GetComponentInParent<Shooter>();
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage, pStats);
            else { Debug.Log("enemy não encontrado"); }
        }
    }

    public void ColiderBlink() 
    {
        StartCoroutine(FastReturn());

    }
    IEnumerator FastReturn() 
    {
        principleCollider.enabled = true;
        yield return new WaitForSeconds(frametime);
        principleCollider.enabled = false;
    }

}
