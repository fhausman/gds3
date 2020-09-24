using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum LensDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class Lens : MonoBehaviour
{
    [SerializeField]
    private LensDirection _dir = LensDirection.UP;

    [SerializeField]
    private BeamColor _reflectColor = BeamColor.BLUE;
    public BeamColor Color { get => _reflectColor; }

    public Vector3 GetReflectionDirection()
    {
        switch(_dir)
        {
            case LensDirection.UP:
                return transform.up;
            case LensDirection.DOWN:
                return -transform.up;
            case LensDirection.LEFT:
                return -transform.right;
            case LensDirection.RIGHT:
                return transform.right;
            default:
                return Vector3.zero;
        }
    }

    public void OnInteractionEnter()
    {
        gameObject.SetActive(false);
    }

    public void OnInteractionEnd()
    {
        gameObject.SetActive(true);
    }
}
