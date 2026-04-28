using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Ranged Weapon")]
public class RangedWeaponData : ScriptableObject
{
    [Header("Estatísticas Principais")] //caracteristicas base que se alteram
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public float range = 10f;
    public int damage = 10;

    [Header("Comportamento do Disparo")] //isso aqui vai mudar os projeteis por tiro e o angulo respectivamente

    public int pellets = 1;
    public float spread = 0f;
}