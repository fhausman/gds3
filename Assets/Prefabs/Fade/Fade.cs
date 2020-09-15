using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    private Animator _anim = null;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    void FadeIn()
    {
        _anim.Play("FadeIn");
    }

    void FadeOut()
    {
        _anim.Play("FadeOut");
    }
}
