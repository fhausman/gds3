using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "GDS/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    public int health = 3;
    public float speed = 0.0f;
    public float gravitySpeed = 0.0f;
    public float blockSpeedModifier = 0.0f;
    public float dashSpeed = 0.0f;
    public float dashTime = 0.0f;
    public float dashCooldown = 0.0f;
    public Vector2 upperBlockZoneSize = Vector2.zero;
    public Vector2 bottomBlockZoneSize = Vector2.zero;
    public float sweetSpotWidth = 0.0f;
    public float attackDuration = 0.0f;
    public float gravitySwitchHeight = 5.0f;
}
