using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField]
    private TextScroller _textScroller = null;

    [SerializeField]
    private AudioClip _explosion = null;

    [SerializeField]
    private AudioClip _laserOff = null;

    [SerializeField]
    private Animator _animator = null;

    private AudioSource _audio;
    private GameObject _laserBeam;

    public void Start()
    {
        _audio = GetComponent<AudioSource>();
        _laserBeam = GameObject.Find("LaserBeam");
    }

    public void Explode()
    {
        StartCoroutine(ExplosionInternal());
    }

    private IEnumerator ExplosionInternal()
    {
        _textScroller.SendMessage("DisallowLoad");
        _animator.enabled = true;
        _audio.PlayOneShot(_explosion, 0.2f);

        yield return new WaitForSeconds(2.0f);

        _audio.PlayOneShot(_laserOff);
        _laserBeam.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        _animator.enabled = false;
        _textScroller.SendMessage("AllowLoad");
    }
}
