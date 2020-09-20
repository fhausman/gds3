using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onCheckPointEnter;
    
    public void OnCheckPointEnter()
    {
        onCheckPointEnter.Invoke();
    }
}
