using Boo.Lang;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ProjectileSettings _settings = null;
    [SerializeField]
    private Collider _collider = null;
    [SerializeField]
    private VisualEffect _sparks = null;
    [SerializeField]
    private GameObject _body = null;

    public bool IsReflected { get; set; } = false;

    private float _elapsedTime = 0.0f;
    private Vector3 _startingPosition = Vector3.zero;
    private float _speed = 0.0f;

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
        _speed = 0.0f;
        _collider.enabled = false;
        StartCoroutine(KillProjectile());
    }

    private void OnCollisionEnter(Collision collision)
    {
        var enemiesLayer = LayerMask.NameToLayer("Enemies");
        if (collision.gameObject.layer == enemiesLayer)
        {
            if (IsReflected)
            {
                collision.gameObject.SendMessage("ReceivedDamage", Dir.x);
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

    private IEnumerator KillProjectile()
    {
        var progress = 0.0f;
        var rate = _sparks.GetFloat("Rate");
        var sparksTransform = _sparks.gameObject.transform;
        sparksTransform.transform.localScale = sparksTransform.transform.localScale * 2;
        while (rate > 0)
        {
            rate = Mathf.Lerp(rate, 0, progress);
            var scale = Vector3.Lerp(_body.transform.localScale, Vector3.zero, progress);
            _body.transform.localScale = scale;

            _sparks.SetFloat("Rate", rate);

            progress += 0.05f;

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.CompareTag("ProjectileShield"))
        //{
        //    if (other.gameObject.transform.parent.gameObject.GetComponentInChildren<Weapon>().IsInSweetSpot(transform.position))
        //    {
        //        Reflect();
        //        return;
        //    }

        //    Destroy();
        //}
    }

    private IEnumerator EnableCol()
    {
        while(_elapsedTime < 0.1f)
        {
            yield return null;
        }

        _collider.enabled = true;
    }

    private void Start()
    {
        _collider.enabled = false;
        _startingPosition = transform.position;
        _speed = _settings.projectileSpeed;

        StartCoroutine(EnableCol());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var deltaTime = Time.deltaTime;

        var moveVec = Dir * _speed;
        transform.Translate(moveVec * deltaTime);

        if (_settings.frequency != 0)
        {
            var sin = transform.up * (_startingPosition.y + Mathf.Sin(_elapsedTime * _settings.frequency) * _settings.magnitude);
            transform.position = new Vector3(transform.position.x, sin.y, transform.position.z);
        }

        _elapsedTime += deltaTime;
    }
}
