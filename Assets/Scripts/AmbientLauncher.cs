using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientLauncher : MonoBehaviour
{
    [SerializeField]
    private AudioClip _onSound;

    [SerializeField]
    private AudioSource _audio;

    private GameObject _ambientSounds;

    void Start()
    {
        _ambientSounds = GameObject.Find("AmbientSounds");
    }

    public void EnableAmbients()
    {
        foreach(Transform child in _ambientSounds.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void PlayOnSound()
    {
        _audio.PlayOneShot(_onSound);
    }
}
