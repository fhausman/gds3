using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ProjectileSettings _settings = null;
    [SerializeField]
    private Collider _collider = null;
    private float _elapsedTime = 0.0f;
    private Vector3 _startingPosition = Vector3.zero;

    public Vector3 Dir { get; set; } = Vector3.zero;

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
        _collider.enabled = false;
        _startingPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var deltaTime = Time.deltaTime;

        var horizontalVec = transform.right * Dir.x * _settings.projectileSpeed;
        transform.Translate(horizontalVec * deltaTime);
        var verticalVec = _startingPosition.y + Mathf.Sin(_elapsedTime * _settings.frequency) * _settings.magnitude;
        transform.position = new Vector3(transform.position.x, verticalVec, transform.position.z);

        _elapsedTime += deltaTime;
        if(_elapsedTime >= 0.1f)
        {
            _collider.enabled = true;
        }
    }
}
