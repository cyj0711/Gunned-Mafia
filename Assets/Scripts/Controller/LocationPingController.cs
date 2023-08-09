using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationPingController : MonoBehaviour
{
    Transform m_vPlayerTransform;
    Transform m_vTargetTransform;
    [SerializeField] Text m_vDistanceText;

    float m_fLifeTime = 10f;    // 위치 핑의 잔재 시간. 따로 설정해주지 않을 경우, 기본값은 10초간 유지됨
    float m_fUpdateTime = 10f;  // m_bIsUpdatingPing 이 true 일 경우, 해당 변수 시간 주기로 위치를 갱신함.
    float m_fTimer = 0f;
    float m_fUpdateTimer = 0f;
    float m_fDistance;

    bool m_bIsUpdatingPing = false; // 주기적으로 위치를 갱신해주는 location ping 인지 확인

    int m_iTargetPlayerActorNumber = -1;

    Vector3 m_vOriginalPosition;

    [SerializeField] Canvas m_vCanvas;
    [SerializeField] TargetIndicator m_vTargetIndicator;


    public void InitData(Transform _vPlayerTransform, Transform _vTargetTransform, float _fLifeTime, bool _bIsUpdatingPing = false)
    {
        m_vPlayerTransform = _vPlayerTransform;
        m_vTargetTransform = _vTargetTransform;
        m_fLifeTime = _fLifeTime;
        m_vOriginalPosition = transform.position;
        m_bIsUpdatingPing = _bIsUpdatingPing;

        m_vCanvas.worldCamera = Camera.main;
        m_vTargetIndicator.SetTarget(m_vOriginalPosition);
    }

    public void SetTargetPlayerActorNumber(int _iTargetPlayerActorNumber)
    {
        m_iTargetPlayerActorNumber = _iTargetPlayerActorNumber;
    }

    public void SetTargetTransform(Transform _vTransform)
    {
        m_vTargetTransform = _vTransform;
    }

    void Update()
    {
        if (m_fTimer >= m_fLifeTime)
            Destroy(gameObject);

        if (m_bIsUpdatingPing)
        {
            if (m_fUpdateTimer >= m_fUpdateTime)
            {
                m_vOriginalPosition = transform.position = m_vTargetTransform.position;
                m_vTargetIndicator.SetTarget(m_vOriginalPosition);
                m_fUpdateTimer = 0f;
            }

            m_fUpdateTimer += Time.deltaTime;
        }

        m_fDistance = Vector2.Distance(m_vPlayerTransform.position, m_vOriginalPosition);
        m_vDistanceText.text = ((int)(m_fDistance * 10)).ToString();

        m_fTimer += Time.deltaTime;
    }

    private void OnDestroy()
    {
        if (m_iTargetPlayerActorNumber != -1)
            UISearchManager.I.RemoveDicPlayerLocationPing(m_iTargetPlayerActorNumber);
    }
}