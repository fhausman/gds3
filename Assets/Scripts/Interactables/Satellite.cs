using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Satellite : MonoBehaviour
{
    [SerializeField]
    private float _moveTime = 1.0f;

    private Vector3 _closePosition = Vector3.zero;
    private Vector3 _openPosition = new Vector3(0.0f, 0.0f, -45.0f);

    public void Open()
    {
        StartCoroutine(OpenInternal());
    }

    IEnumerator OpenInternal()
    {
        var startTime = Time.time;
        while (Time.time - startTime < _moveTime)
        {
            var new_rot = Quaternion.Lerp(Quaternion.Euler(_closePosition), Quaternion.Euler(_openPosition), (Time.time - startTime) / _moveTime);
            transform.rotation = new_rot;

            yield return null;
        }
    }

}
