using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    void Start()
    {
        Camera.main.eventMask = LayerMask.GetMask("MouseEvent");
    }

}
