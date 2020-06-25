using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserPulse : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _maxWidth = 1.0f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _minWidth = 0.5f;

    [SerializeField]
    private float _frequency = 60;

    private LineRenderer _lineRenderer = null;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        var _currentWidth = _minWidth + (_maxWidth - _minWidth) * Mathf.Abs(Mathf.Sin(Time.time * _frequency));
        _lineRenderer.startWidth = _currentWidth;
        _lineRenderer.endWidth = _currentWidth;
    }
}
