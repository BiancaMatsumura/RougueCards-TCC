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

    public float CoolDownBetween;
    
    
    [Header("Combo System")]
    public int ComboLengthMax = 3;
    public float ComoboWindow = 2f;

    public bool IsShooter;
    public GameObject WeaponPrefab;
    public GameObject SlashPrefab;
    public Transform FrontPosition;
    public Transform HandsPosition;
    
    private Health life;
    public Damage damage;

    [Header("SlashVfx")]
    //public GameObject slashprefab;
    private Animator slashanimation;
    void Start()
    {
        InitWeaponMesh(weaponData);
        InitSlashMesh(weaponData);
        ApplyWeaponAnimations();
        life = GetComponent<Health>();
       // slashanimation = slashprefab.GetComponent<Animator>();
      

        if (weaponData != null)
        {
            ComboLengthMax = weaponData.comboMax;
            ComoboWindow *= weaponData.comboWindowMultiplier;
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
        StartCoroutine(BeguinAtack());
       
    }
    IEnumerator BeguinAtack()
    {
        yield return new WaitForSeconds(CoolDownBetween);
        counter++;
        SlashVfx();
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

    public void SwichWeaponMesh(MeleeWeaponData weaponData) 
    {
        
        if (IsShooter)
        {
            if (WeaponPrefab != null)
            {
                damage.damage = weaponData.baseDamage;
                DestroyImmediate(WeaponPrefab,true);
            }
            if (weaponData.WeaponPrefab != null)
            {
                
                WeaponPrefab = Instantiate(weaponData.WeaponPrefab, HandsPosition);
                damage.damage = weaponData.baseDamage;
            }
            else { Debug.Log("Nao tem prefab droga!"); }
            }
        }

    public void SwichSlahsMesh(MeleeWeaponData weaponData) 
    {
        
        if (IsShooter)
        {
            if (SlashPrefab != null)
            {
                DestroyImmediate(SlashPrefab,true);
            }
            if (weaponData.SlashPrefab != null)
            {
                SlashPrefab = Instantiate(weaponData.SlashPrefab, FrontPosition);
                slashanimation = SlashPrefab.GetComponent<Animator>();
            }
            else { Debug.Log("Nao tem prefab droga!"); }
            }
        }

    public void InitWeaponMesh(MeleeWeaponData weaponData) 
    {
        if (weaponData.WeaponPrefab != null)
        {
            
            WeaponPrefab = Instantiate(weaponData.WeaponPrefab, HandsPosition);
             damage.damage = weaponData.baseDamage;
            
        }
        else { Debug.Log("Nao tem prefab droga!"); }
    }
    public void InitSlashMesh(MeleeWeaponData weaponData) 
    {
        if (weaponData.SlashPrefab != null)
        {
            SlashPrefab = Instantiate(weaponData.SlashPrefab, FrontPosition);
            slashanimation = SlashPrefab.GetComponent<Animator>();
        }
        else { Debug.Log("Nao tem prefab droga!"); }
    }

    void SlashVfx()
    {
            slashanimation.Rebind();
    }
}
