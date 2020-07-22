using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MeleeWeapon : MonoBehaviour
{
    [SerializeField]
    private BoxCollider col = null;

    private void Enable()
    {
        col.enabled = true;
    }

    private void Disable()
    {
        col.enabled = false;
    }
}
