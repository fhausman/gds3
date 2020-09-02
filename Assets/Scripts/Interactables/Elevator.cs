using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private enum Direction
    {
        HORIZONTAL,
        VERTICAL
    };

    [SerializeField]
    private float _moveSpeed = 1.0f;

    [SerializeField]
    private Direction _direction = Direction.HORIZONTAL;

    private Rigidbody _rb = null;
    private Vector3 _dir = Vector3.left;

    private float delta = 0.0f;

    void Awake()
    {
        _dir = _direction == Direction.HORIZONTAL ? Vector3.left : Vector3.up;

        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _rb.velocity = _dir * _moveSpeed;

        delta += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (delta < 0.05f)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _dir = -_dir;
            delta = 0.0f;
        }
    }
}
