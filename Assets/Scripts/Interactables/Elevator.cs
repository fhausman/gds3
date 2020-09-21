using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
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

    [SerializeField]
    private float _upDownBounceDistance = 2.05f;

    [SerializeField]
    private float _leftRightBounceDistance = 2.05f;


    private Vector3 _dir = Vector3.left;

    private float delta = 0.0f;

    void OnEnable()
    {
        _dir = _direction == Direction.HORIZONTAL ? Vector3.left : Vector3.up;
    }

    private void FixedUpdate()
    {
        transform.position += _dir * _moveSpeed * Time.fixedDeltaTime;
        if (Physics.Raycast(transform.position, _dir, (Direction.HORIZONTAL == _direction ? _leftRightBounceDistance : _upDownBounceDistance), LayerMask.GetMask("Ground")))
        {
            _dir = -_dir;
        }
    }
}
