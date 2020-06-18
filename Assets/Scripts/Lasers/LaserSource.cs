using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum LaserType
{
    DeadlyLaser,
    PowerBeam
}

public interface ILaserHitBehaviour
{
    bool OnObjectHit(GameObject hit_object);
    int GetCollisionMask();
}

public class DeadlyLaserBehaviour : ILaserHitBehaviour
{
    private int _layerMask = 0;

    public DeadlyLaserBehaviour()
    {
        _layerMask = LayerMask.GetMask("Player", "Ground");
    }

    public int GetCollisionMask()
    {
        return _layerMask;
    }

    public bool OnObjectHit(GameObject hit_object)
    {
        if(hit_object.CompareTag("Player"))
        {
            hit_object.SendMessage("OnLaserHit");
            return true;
        }

        return false;
    }
}

public class PowerBeamBehaviour : ILaserHitBehaviour
{
    private int _layerMask = 0;

    public PowerBeamBehaviour()
    {
        _layerMask = LayerMask.GetMask("Lens", "Ground");
    }

    public int GetCollisionMask()
    {
        return _layerMask;
    }

    public bool OnObjectHit(GameObject hit_object)
    {
        return false;
    }
}

[ExecuteInEditMode]
public class LaserSource : MonoBehaviour
{
    [SerializeField]
    private GameObject _beamOrigin;

    [SerializeField]
    private LineRenderer _beam;

    [SerializeField]
    private LaserType _laserType;

    private ILaserHitBehaviour _laserHitBehaviour;
    private const int MAX_REFLECTIONS_NUM = 10;

    private void Awake()
    {
        switch (_laserType)
        {
            case LaserType.DeadlyLaser:
                _laserHitBehaviour = new DeadlyLaserBehaviour();
                break;
            case LaserType.PowerBeam:
                _laserHitBehaviour = new PowerBeamBehaviour();
                break;
        }
    }

    private void Update()
    {
        var beamPoints = new List<Vector3>();
        var currentPoint = _beamOrigin.transform.position;
        var currentDir = _beamOrigin.transform.right;
        beamPoints.Add(currentPoint);

        for(int i = 0; i < MAX_REFLECTIONS_NUM; ++i)
        {
            RaycastHit hit;
            if(!Physics.Raycast(currentPoint, currentDir, out hit, 100.0f, _laserHitBehaviour.GetCollisionMask()))
            {
                break;
            }

            beamPoints.Add(hit.point);
            if (_laserHitBehaviour.OnObjectHit(hit.collider.gameObject))
            {
                break;
            }

            //beam hits orthogonal wall
            if (Vector3.Dot(hit.normal, currentDir.normalized) == -1)
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
