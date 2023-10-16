using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightManager : Singleton<LightManager>
{
    [SerializeField] Light2D m_vGlobalLight;
    [SerializeField] Light2D m_vPlayerLight;

    void Start()
    {
        m_vGlobalLight.intensity = 1f;
        m_vPlayerLight.intensity = 0f;
    }

    public void SetPlayerLightPosition(Transform _vPlayer)
    {
        m_vPlayerLight.transform.parent = _vPlayer;
        m_vPlayerLight.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

    // 살아있는 플레이어는 자신의 주변만 밝게 보인다.
    public void AlivePlayerLight()
    {
        m_vGlobalLight.intensity = 0f;
        m_vPlayerLight.intensity = 1f;
    }

    // 죽은 플레이어는 모든 맵이 밝게 보인다.
    public void DeadPlayerLight()
    {
        m_vGlobalLight.intensity = 1f;
        m_vPlayerLight.intensity = 0f;
    }

}
