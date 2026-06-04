using UnityEngine;

[CreateAssetMenu(fileName = "ArcanoMaior", menuName = "Boss/ArcanoMaior")]
public class ArcanoMaior : ScriptableObject
{
    public string bossName;
    public EnemyData bossEnemyData;
    public int xpStageRequired;
    public int killsRequired;
    public float timeRequired;

}
