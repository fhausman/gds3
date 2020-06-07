using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "GDS/EnemySettings")]
public class EnemySettings : ScriptableObject
{
    public int health = 0;
    public float speed = 0.0f;
    public float shootRate = 0.0f;
    public float range = 0.0f;
    public float safeDistance = 0.0f;
    public float dashForce = 0.0f;
}
