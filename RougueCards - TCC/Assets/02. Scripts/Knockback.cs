using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public Rigidbody rigidbodys;
    public float force;
    public float Duration;
    void Start()
    {
        rigidbodys = gameObject.GetComponent<Rigidbody>();
    }
    public IEnumerator ApplyKnockback(Vector3 direction)
    {
        var oldlinear = rigidbodys.linearDamping;
        var oldangular = rigidbodys.angularDamping;

        rigidbodys.linearDamping = 0.3f;
        rigidbodys.angularDamping = 0.3f;
        rigidbodys.mass = 0.5f;

        rigidbodys.AddForce(direction*force, ForceMode.Impulse);

        yield return new WaitForSeconds(Duration);
        
        rigidbodys.linearDamping = oldlinear;
        rigidbodys.angularDamping = oldangular;
        rigidbodys.mass = 1;

        yield return false;

    }
     public IEnumerator ApplyKnockbackPerson(Vector3 direction, float force2, float Duration2)
    {
        var oldlinear = rigidbodys.linearDamping;
        var oldangular = rigidbodys.angularDamping;

        rigidbodys.linearDamping = 0.3f;
        rigidbodys.angularDamping = 0.3f;
        rigidbodys.mass = 0.5f;

        rigidbodys.AddForce(direction*force2, ForceMode.Impulse);

        yield return new WaitForSeconds(Duration2);
        
        rigidbodys.linearDamping = oldlinear;
        rigidbodys.angularDamping = oldangular;
        rigidbodys.mass = 1;

        yield return false;

    }
}
