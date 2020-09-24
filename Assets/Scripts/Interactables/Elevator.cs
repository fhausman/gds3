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
    private float _upBounceDistance = 2.05f;

    [SerializeField]
    private float _downBounceDistance = 2.05f;

    [SerializeField]
    private float _leftBounceDistance = 2.05f;

    [SerializeField]
    private float _rightBounceDistance = 2.05f;


    private Vector3 _dir = Vector3.left;

    private float delta = 0.0f;

    void OnEnable()
    {
        _dir = _direction == Direction.HORIZONTAL ? Vector3.left : Vector3.up;
    }

    private void FixedUpdate()
    {
        transform.position += _dir * _moveSpeed * Time.fixedDeltaTime;
        if (Physics.Raycast(transform.position, _dir, GetBounceDistance(), LayerMask.GetMask("Ground")))
        {
            _dir = -_dir;
        }
    }

    private float GetBounceDistance()
    {
        if (Direction.HORIZONTAL == _direction)
        {
            return _dir.x > 0.0f ? _rightBounceDistance : _leftBounceDistance;
        }
        else
        {
            return _dir.y > 0.0f ? _upBounceDistance : _downBounceDistance;
        }
    }
}
