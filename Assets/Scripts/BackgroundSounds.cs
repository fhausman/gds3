using System.Collections;
using UnityEngine;

public class BackgroundSounds : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audio;

    [SerializeField]
    private AudioClip[] _sequence;

    private void OnEnable()
    {
        StartCoroutine(PlaySequence());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator PlaySequence()
    {
        int i = 0;

        while(true)
        {
            _audio.PlayOneShot(_sequence[i % _sequence.Length]);

            yield return new WaitForSeconds(8.0f);

            ++i;
        }
    }
}
