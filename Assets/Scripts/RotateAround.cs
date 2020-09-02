using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    [SerializeField]
    private float _speed = 0.0f;

    [SerializeField]
    private Transform _refPoint = null;

    void Update()
    {
        transform.RotateAround(_refPoint.position, Vector3.forward, _speed * Time.deltaTime);
    }
}
