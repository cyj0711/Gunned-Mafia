using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera m_vCameraMain;
    [SerializeField] private CinemachineVirtualCamera m_vCinemachineCamera;
    [SerializeField] private Transform m_vCameraFollower;
    public Transform a_vCameraFollower { get => m_vCameraFollower; }
    void Start()
    {
        Camera.main.eventMask = LayerMask.GetMask("MouseEvent");

        m_vCinemachineCamera.Follow = m_vCameraFollower;
        m_vCinemachineCamera.LookAt = m_vCameraFollower;
    }

    public void SetCinemachineCameraFollowAt(Transform _vTarget)
    {
        m_vCameraFollower.parent = _vTarget;
        m_vCameraFollower.localPosition = new Vector3(0f, 0f);
    }

    public void SetCameraFollowerPosition(Vector3 _vPosition)
    {
        m_vCameraFollower.localPosition = _vPosition;
    }
}
