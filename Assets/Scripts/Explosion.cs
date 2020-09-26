using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField]
    private AudioClip _explosion = null;

    [SerializeField]
    private AudioClip _laserOff = null;

    [SerializeField]
    private Animator _animator = null;

    private AudioSource _audio;

    public void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    public void Explode()
    {
        StartCoroutine(ExplosionInternal());
    }

    private IEnumerator ExplosionInternal()
    {
        _animator.enabled = true;
        _audio.PlayOneShot(_explosion, 0.2f);

        yield return new WaitForSeconds(2.0f);

        _audio.PlayOneShot(_laserOff);

        yield return new WaitForSeconds(0.2f);

        _animator.enabled = false;
    }
}
