using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Ranged Weapon")]
public class RangedWeaponData : ScriptableObject
{
    [Header("Estatísticas Principais")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public float range = 10f;
    public int damage = 10;
    public float lifetime = 3f;

    [Header("Comportamento do Disparo")]
    public BulletMoviment BM;
    public bool DestroyOnContact = true;
    public int pellets = 1;
    public float spread = 0f;

    [Header("Recoil")]
    [Tooltip("Força que empurra o jogador para trás ao disparar")]
    public float recoilForce = 0f;
}