using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserInterval : MonoBehaviour
    //Made by Kamil :D
{

     public GameObject[] lasers;
    [SerializeField] public float repeatRate = 1.0f;
    [SerializeField] private Light _light = null;
    [SerializeField] private AudioClip[] _audio;
    private AudioSource _audioSource;
   
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        InvokeRepeating("EnableRepeat", 1.0f, repeatRate);
    }

    void EnableRepeat()
    {

        bool enabled = false;
        foreach(GameObject lasers in lasers)
        {
            lasers.GetComponent<LineRenderer>().enabled = !lasers.GetComponent<LineRenderer>().enabled;
            lasers.GetComponent<LaserSource>().enabled = !lasers.GetComponent<LaserSource>().enabled;
            lasers.GetComponent<BoxCollider>().enabled = !lasers.GetComponent<BoxCollider>().enabled;
            enabled = lasers.GetComponent<LineRenderer>().enabled;
        }

        if(_light)
        {
            _light.enabled = !_light.enabled;
        }

        if (!enabled)
        {
            _audioSource.Stop();
            _audioSource.PlayOneShot(_audio[2]);
            StartCoroutine(OnSound());
        }
        else
        {
            _audioSource.clip = _audio[1];
            _audioSource.Play();
        }
    }

    IEnumerator OnSound()
    {
        yield return new WaitForSeconds(repeatRate - _audio[0].length);

        _audioSource.PlayOneShot(_audio[0]);
    }
}
