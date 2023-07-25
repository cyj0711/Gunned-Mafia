using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera m_vCameraMain;
    void Start()
    {
        Camera.main.eventMask = LayerMask.GetMask("MouseEvent");
    }
}
