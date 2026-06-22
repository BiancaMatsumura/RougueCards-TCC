using UnityEngine;

/// <summary>
/// ScriptableObject que define o "DNA" e os atributos base de um inimigo.
/// Permite a criação modular de diferentes tipos de oponentes sem a necessidade de novos scripts.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Gameplay/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identidade")]
    /// <summary> Nome de exibição ou identificação do inimigo. </summary>
    public string enemyName;

    /// <summary> O Prefab contendo o modelo 3D (Mesh) que será instanciado visualmente. </summary>
    public GameObject visualPrefab;

    /// <summary> O controlador de animações (Animator) que define o comportamento visual do inimigo. </summary>
    public RuntimeAnimatorController animatorController;

    [Header("Atributos Base")]
    /// <summary> Velocidade de movimento do inimigo durante a perseguição. </summary>
    public float speed = 3f;

    /// <summary> Quantidade máxima de pontos de vida (HP) que o inimigo terá ao surgir. </summary>
    public int maxHealth = 50;

    /// <summary> Quantidade de dano fixo que o inimigo causa ao jogador em cada intervalo de contato. </summary>
    public int damageToPlayer = 10;

    /// <summary> Quantidade de experiência (XP) que será gerada e concedida ao jogador após a morte deste inimigo. </summary>
    public int xpValue = 20;

    [Header("Configuração de Escala")]
    /// <summary> Multiplicador de escala para ajustar o tamanho do modelo visual em relação ao objeto pai. </summary>
    public float modelScale = 1f;

    [Header("Audio")]
    /// <summary> Som tocado quando o inimigo recebe dano. </summary>
    public AudioClip damageSound;

    /// <summary> Som tocado quando o inimigo morre. </summary>
    public AudioClip deathSound;
}