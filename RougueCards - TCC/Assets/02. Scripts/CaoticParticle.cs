using System.Collections;
using UnityEngine;

public class CaoticParticle : MonoBehaviour
{

    Health health;
    public int Damage;
    public float speed = 1.0f;
    public ParticleSystem PS;
    bool moving = true;
    AudioSource Audio;
    public float DestroyCowldown;
    void Start()
    {
        Audio = GetComponent<AudioSource>();
        PS = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Colidiu");

        if (health = other.gameObject.GetComponent<Health>())
        {
            health.TakeDamage(Damage);
        }
        if (other.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Colidiu c chao");
            moving = false;
            PS.Play();
            Audio.Play();

            StartCoroutine(Destroyer());
        }
        
    }
   
     public IEnumerator Destroyer()  
    {
        yield return new WaitForSeconds(DestroyCowldown);
        Destroy(this.gameObject);
        yield return null;
       
    
    }
    private void FixedUpdate()
    {
        if (moving)
        {
            transform.position = transform.position + Vector3.down * speed * Time.deltaTime;
        }
    }
}
