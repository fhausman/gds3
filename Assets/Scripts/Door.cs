using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private float _moveTime = 1.0f;

    private Vector3 _closePosition = Vector3.zero;
    private Vector3 _openPosition = Vector3.zero;

    private void Start()
    {
        _closePosition = transform.position;
        _openPosition = _closePosition - new Vector3(0.0f, 0.0f, -1.0f);
    }

    public void Open()
    {
        StopAllCoroutines();
        StartCoroutine(Move(transform.position, _openPosition));
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(Move(transform.position, _closePosition));
    }

    private IEnumerator Move(Vector3 from, Vector3 target)
    {
        var startTime = Time.time;
        while(Time.time - startTime < _moveTime)
        {
            var new_z = Mathf.Lerp(from.z, target.z, (Time.time - startTime) / _moveTime);
            transform.position = new Vector3(transform.position.x, transform.position.y, new_z);

            yield return null;
        }
    }
}
