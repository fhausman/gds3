using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _onInteractionStart;

    [SerializeField]
    private UnityEvent _onInteractionEnd;

    public void OnInteractionStart()
    {
        _onInteractionStart.Invoke();
    }

    public void OnInteractionEnd()
    {
        _onInteractionEnd.Invoke();
    }
}
