using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationPingController : MonoBehaviour
{
    Transform m_vPlayerTransform;
    [SerializeField] Text m_vDistanceText;

    float m_fLifeTime = 10f;    // 위치 핑의 잔재 시간. 따로 설정해주지 않을 경우, 기본값은 10초간 유지됨
    float m_fTimer = 0f;
    float m_fDistance;


    public void InitData(Transform _vPlayerTransform, float _fLifeTime)
    {
        m_vPlayerTransform = _vPlayerTransform;
        m_fLifeTime = _fLifeTime;
    }
    void Update()
    {
        if (m_fTimer >= m_fLifeTime)
            Destroy(gameObject);

        m_fDistance = Vector2.Distance(m_vPlayerTransform.position, transform.position);
        m_vDistanceText.text = ((int)(m_fDistance * 10)).ToString();

        m_fTimer += Time.deltaTime;
    }
}
