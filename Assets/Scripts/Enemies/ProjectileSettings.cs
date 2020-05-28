using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSettings", menuName = "GDS/ProjectileSettings")]
public class ProjectileSettings : ScriptableObject
{
    public float projectileSpeed = 0.0f;
    public float magnitude = 0.0f;
    public float frequency = 0.0f;
}
