using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private float _moveTime = 1.0f;

    [SerializeField]
    private bool _locked = false;

    private Vector3 _closePosition = Vector3.zero;
    private Vector3 _openPosition = Vector3.zero;

    [SerializeField]
    private Renderer lampRef = null;

    [SerializeField]
    private Material closedMaterial = null;

    [SerializeField]
    private Material openedMaterial = null;

    [SerializeField]
    private AudioClip _openSound = null;

    [SerializeField]
    private AudioClip _closeSound = null;

    private AudioSource _audio = null;

    private void Start()
    {
        _closePosition = transform.position;
        _openPosition = _closePosition - new Vector3(0.0f, 0.0f, -2.0f);
        _audio = GetComponent<AudioSource>();
    }

    public void Open()
    {
        if (!_locked)
        {
            lampRef.material = openedMaterial;
            StopAllCoroutines();
            StartCoroutine(Move(transform.position, _openPosition));
            _audio.Stop();
            _audio.PlayOneShot(_openSound);
        }
    }

    public void Close()
    {
        if (!_locked)
        {
            lampRef.material = closedMaterial;
            StopAllCoroutines();
            StartCoroutine(Move(transform.position, _closePosition));
            _audio.Stop();
            _audio.PlayOneShot(_closeSound);
        }
    }

    private IEnumerator Move(Vector3 from, Vector3 target)
    {
        var startTime = Time.time;
        while(Time.time - startTime < _moveTime)
        {
            var new_z = Mathf.Lerp(from.z, target.z, (Time.time - startTime) / (_moveTime * Mathf.Abs((target.z - from.z) / (_closePosition.z - _openPosition.z))));
            transform.position = new Vector3(transform.position.x, transform.position.y, new_z);

            yield return null;
        }
    }
}
