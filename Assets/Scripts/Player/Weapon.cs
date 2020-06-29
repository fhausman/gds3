using UnityEngine;
using UnityEngine.Rendering;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private BoxCollider _collider = null;

    private Vector3 upperPosition = new Vector3(0.205f, 0.679f, 0.0f);
    private Vector3 upperRotation = new Vector3(0.0f, 0.0f, -51.686f);

    private Vector3 bottomPosition = new Vector3(0.181f, 0.359f, 0.0f);
    private Vector3 bottomRotation = new Vector3(0.0f, 0.0f, -302.945f);

    private Vector3 idlePosition = new Vector3(-0.209f, 0.55f, 0.0f);
    private Vector3 idleRotation = new Vector3(0.0f, 0.0f, -358.283f);
    
    private void SetTransform(Vector3 pos, Vector3 rot)
    {
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Euler(rot);
    }

    public void SetUpper()
    {
        _collider.enabled = true;
        SetTransform(upperPosition, upperRotation);
    }

    public void SetBottom()
    {
        _collider.enabled = true;
        SetTransform(bottomPosition, bottomRotation);
    }

    public void SetIdle()
    {
        _collider.enabled = false;
        SetTransform(idlePosition, idleRotation);
    }

    public void EnableCollision()
    {
        _collider.enabled = true;
    }

    public void DisableCollision()
    {
        _collider.enabled = false;
    }
}
