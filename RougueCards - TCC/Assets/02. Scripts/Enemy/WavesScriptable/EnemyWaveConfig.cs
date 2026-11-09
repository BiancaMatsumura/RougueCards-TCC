using UnityEngine;


[CreateAssetMenu(fileName = "EnemyWaveConfig", menuName = "Scriptable Objects/EnemyWaveConfig")]
public class EnemyWaveConfig : ScriptableObject
{
    [SerializeField] public string Wavename;
    [SerializeField] public GameObject[] prefabsInimigos;
    [SerializeField] public EnemyData[] dadosInimigosDisponiveis;
    [SerializeField] public bool HasCaoticEvents;
    [SerializeField] public GameObject[] CaoticGameObjects;



}
