using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera _vcam = null;

    [SerializeField]
    private Player _playerRef = null;

    [SerializeField]
    private float _lensShift = 0.12f;

    [SerializeField]
    private float _followOffset = 1.0f;

    private CinemachineTransposer _transposer = null;

    private Vector2 LensShift { get => _vcam.m_Lens.LensShift; set => _vcam.m_Lens.LensShift = value; }
    private Vector3 FollowOffset { get => _transposer.m_FollowOffset; set => _transposer.m_FollowOffset = value; }
    private Vector3 PlayerUpVec { get => _playerRef.transform.up; }
    private Vector3 PlayerRightVec { get => _playerRef.transform.right; }


    private void Start()
    {
        _transposer = _vcam.GetCinemachineComponent<CinemachineTransposer>();
    }

    void Update()
    {
        var dot = Vector3.Dot(Vector3.up, PlayerUpVec);
        
        var newShift = new Vector2(0.0f, _lensShift * dot);
        LensShift = Vector2.Lerp(LensShift, newShift, 0.1f);

        var newOffset = new Vector3(0.0f, _followOffset * dot, -10.0f);
        FollowOffset = Vector3.Lerp(FollowOffset, newOffset, 0.1f);
    }
}
