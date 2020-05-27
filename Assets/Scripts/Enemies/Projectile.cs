using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ProjectileSettings settings = null;
    //private Rigidbody rb;
    private Vector3 _dir = Vector3.zero;
    private float _elapsedTime = 0.0f;

    private void Reflect()
    {
        Debug.Log("Reflect");
        _dir = -_dir;
    }

    private void Destroy()
    {
        Debug.Log("Destroy");
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy();
    }

    private void Start()
    {
        _dir = -transform.right;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var deltaTime = Time.deltaTime;

        var horizontalVec = _dir * settings.projectileSpeed;
        var verticalVec = Vector3.up * Mathf.Sin(_elapsedTime * settings.frequency) * settings.magnitude;
        transform.Translate((horizontalVec + verticalVec) * deltaTime);

        _elapsedTime += deltaTime;
    }
}
