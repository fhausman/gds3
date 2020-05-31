using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ProjectileSettings settings = null;
    [SerializeField]
    private Collider collider = null;

    public Vector3 Dir { get; set; } = Vector3.zero;
    private float _elapsedTime = 0.0f;

    private void Reflect()
    {
        Debug.Log("Reflect");
        Dir = -Dir;
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
        collider.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var deltaTime = Time.deltaTime;

        var horizontalVec = transform.right * Dir.x * settings.projectileSpeed;
        var verticalVec = transform.up * Dir.y * Mathf.Sin(_elapsedTime * settings.frequency) * settings.magnitude;
        transform.Translate((horizontalVec + verticalVec) * deltaTime);

        _elapsedTime += deltaTime;
        if(_elapsedTime >= 0.1f)
        {
            collider.enabled = true;
        }
    }
}
