using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashControl : MonoBehaviour
{
    private Animator anim;
    public float timedelay;
    void Start()
    {
        anim = this.GetComponent<Animator>();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            StopAllCoroutines();
            StartCoroutine(Dash());
           
        }
    }
    public IEnumerator Dash()
    {
        anim.applyRootMotion = true;
        anim.Play("Dash");
        yield return new WaitForSeconds(timedelay);
        anim.applyRootMotion = false;

    }
}
