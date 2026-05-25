using UnityEngine;

public class EnemyXP : MonoBehaviour
{
    [SerializeField] private GameObject XPprefab;
    private int _xpValue;
    private Health health;

    void Awake() => health = GetComponent<Health>();

    public void SetXPValue(int val) => _xpValue = val;

    void OnEnable()
    {
        if (health != null)
        { health.OnDeath += GiveXP; }
    }
    void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath -= GiveXP;
        }
    }

    void GiveXP()
    {
        if (XPprefab != null)
        {
            GameObject xpObj = Instantiate(XPprefab, transform.position, Quaternion.identity);
            // Se o seu script de XP precisar saber o valor:
             //xpObj.GetComponent<XP>().SetAmount(_xpValue);
        }
    }
}
