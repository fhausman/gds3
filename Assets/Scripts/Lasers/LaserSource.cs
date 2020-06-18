using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LaserSource : MonoBehaviour
{
    [SerializeField]
    private GameObject _beamOrigin;

    [SerializeField]
    private LineRenderer _beam;

    private const int MAX_REFLECTIONS_NUM = 10;

    void Update()
    {
        var beamPoints = new List<Vector3>();
        var currentPoint = _beamOrigin.transform.position;
        var currentDir = _beamOrigin.transform.right;
        beamPoints.Add(currentPoint);

        for(int i = 0; i < MAX_REFLECTIONS_NUM; ++i)
        {
            RaycastHit hit;
            if(!Physics.Raycast(currentPoint, currentDir, out hit, 100.0f, 1 << 8))
            {
                break;
            }

            beamPoints.Add(hit.point);

            //beam hits orthogonal wall
            if(Vector3.Dot(hit.normal, currentDir.normalized) == -1)
            {
                break;
            }

            currentDir = Vector3.Reflect(currentDir, hit.normal);
            currentPoint = hit.point;
        }

        _beam.positionCount = beamPoints.Count;
        _beam.SetPositions(beamPoints.ToArray());
    }


}
