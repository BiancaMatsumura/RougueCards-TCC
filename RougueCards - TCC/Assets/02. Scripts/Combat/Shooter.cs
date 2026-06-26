using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    [Header("Animator")]
    public Animator control;

    [Header("Weapon Data")]
    public MeleeWeaponData weaponData;

    // sistema de combo
    private bool IsInCombo;
    public  int counter;

    [Header("Combo System")]
    public int ComboLengthMax = 3;
    public float ComoboWindow = 2f;

    public bool IsShooter;
    public GameObject Hands;
    private Health life;
    private Damage damage;

    void Start()
    {
        ApplyWeaponAnimations();
        damage = GetComponentInChildren<Damage>();
        life = GetComponent<Health>();

      

        if (weaponData != null)
        {
            ComboLengthMax = weaponData.comboMax;
            ComoboWindow *= weaponData.comboWindowMultiplier;
        }

        if (IsShooter && Hands != null)
        {
            Hands.SetActive(true);
        }
    }

    public void ATK(InputAction.CallbackContext context)
    {
        if (!context.performed || life._isDead) return;

        if (!IsInCombo)
        {
            StartCoroutine(ComboTimer());
            Debug.Log("combo start");
        }

        counter++;
        ComboSteps(ComboLengthMax);
        Debug.Log(counter);
    }

    IEnumerator ComboTimer()
    {
        Debug.Log("combo true");
        IsInCombo = true;

        yield return new WaitForSeconds(ComoboWindow);

        counter = 0;
        BacktoTrigger();

        IsInCombo = false;

        Debug.Log("time out Corroutine off");
    }

    void ComboSteps(int StepsLength)
    {
        
        if (!IsInCombo) return;

        if (counter > StepsLength)
        {
            counter = 0;
            BacktoTrigger();
            return;
        }

       
        string Ataque = "Atack";
        string Next = Ataque + counter;

        control.Play(Next);
        damage.ColiderBlink();
        // FUTURO: dano aqui pode usar weaponData.baseDamage
    }

    void BacktoTrigger()
    {
   
        control.SetTrigger("Back");
        
    }
    public void ApplyWeaponAnimations() 
    {
        if (control != null && weaponData != null)
        {
            weaponData.ApplyOverride(control);
        }
    }
}