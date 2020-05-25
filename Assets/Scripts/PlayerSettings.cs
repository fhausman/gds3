using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "GDS/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    public float speed = 0.0f;
    public float gravitySpeed = 0.0f;
    public float dashSpeed = 0.0f;
    public float dashTime = 0.0f;
}
