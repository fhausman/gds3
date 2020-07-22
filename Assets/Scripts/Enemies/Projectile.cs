using Boo.Lang;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ProjectileSettings _settings = null;
    [SerializeField]
    private Collider _collider = null;

    public bool IsReflected { get; set; } = false;

    private float _elapsedTime = 0.0f;
    private Vector3 _startingPosition = Vector3.zero;

    public Vector3 Dir { get; set; } = Vector3.zero;

    public List<Collider> _ignoredColliders = new List<Collider>();

    private void Reflect()
    {
        Debug.Log("Reflect");
        Dir = -Dir;
        IsReflected = true;

        foreach(var coll in _ignoredColliders)
        {
            Physics.IgnoreCollision(coll, _collider, false);
        }
    }

    private void Destroy()
    {
        Debug.Log("Destroy");
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var enemiesLayer = LayerMask.NameToLayer("Enemies");
        if (collision.gameObject.layer == enemiesLayer)
        {
            if (IsReflected)
            {
                collision.gameObject.SendMessage("ReceivedDamage", Dir);
                Destroy();
            }
            else
            {
                _ignoredColliders.Add(collision.collider);
                Physics.IgnoreCollision(collision.collider, _collider, true);
            }
        }
        else
        {
            Destroy();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("ProjectileShield"))
        {
            if (other.gameObject.transform.parent.gameObject.GetComponentInChildren<Weapon>().IsInSweetSpot(transform.position))
            {
                Reflect();
                return;
            }

            Destroy();
        }
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

        var moveVec = Dir * _settings.projectileSpeed;
        transform.Translate(moveVec * deltaTime);

        if (_settings.frequency != 0)
        {
            var sin = transform.up * (_startingPosition.y + Mathf.Sin(_elapsedTime * _settings.frequency) * _settings.magnitude);
            transform.position = new Vector3(transform.position.x, sin.y, transform.position.z);
        }

        if(_elapsedTime >= 0.1f)
        {
            _collider.enabled = true;
        }

        _elapsedTime += deltaTime;
    }
}
