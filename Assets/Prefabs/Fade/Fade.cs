using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class Fade : MonoBehaviour
{
    private Animator _anim = null;

    public UnityEvent onFadeOutEnd { get; } = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public void FadeIn()
    {
        _anim.Play("FadeIn");
    }

    public void FadeOut()
    {
        _anim.Play("FadeOut");
    }

    public void OnFadeOutEnd()
    {
        onFadeOutEnd.Invoke();
    }
}
