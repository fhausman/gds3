using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 1.0f;

    private Rigidbody _rb = null;
    private Vector3 _dir = Vector3.left;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _rb.velocity = _dir * _moveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var hwdp = LayerMask.GetMask("Ground");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _dir = -_dir;
        }
    }
}
