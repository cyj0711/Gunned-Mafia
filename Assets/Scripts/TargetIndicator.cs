using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// source by https://www.youtube.com/watch?v=265lCMTNMQI (Unity 2D Target Indicator Without Canvas: On-screen & Off-screen Pointers by https://www.youtube.com/@codewithk)

public class TargetIndicator : MonoBehaviour
{
    private Vector3 m_vTargetPosition;
    //public float offScreenThreshold = 10f;
    private Camera m_vMainCamera;
    private float m_fSmoothLerpSpeed = 10f;
    //private bool m_bIsIndicatorActive = true;

    void Start()
    {
        m_vMainCamera = Camera.main;
    }

    public void SetTarget(Vector3 _target)
    {
        m_vTargetPosition = _target;
    }

    void Update()
    {
        //if(m_bIsIndicatorActive)
        //{
        //Vector3 targetDirection = m_vTargetPosition - m_vMainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, m_vMainCamera.nearClipPlane));
        //float distanceToTarget = targetDirection.magnitude;

        //if (distanceToTarget < offScreenThreshold)
        //{
        //    //gameObject.SetActive(false);
        //    //isIndicatorActive = false;
        //}
        //else
        //{
        Vector3 targetViewPortPosition = m_vMainCamera.WorldToViewportPoint(m_vTargetPosition);

        // OnScreen
        if (targetViewPortPosition.z > 0 && targetViewPortPosition.x > 0 && targetViewPortPosition.x < 1 && targetViewPortPosition.y > 0 && targetViewPortPosition.y < 1)
        {
            //gameObject.SetActive(false);

            //transform.position = m_vTargetPosition;
            SmoothLerp(m_vTargetPosition);
        }
        //  OffScreen
        else
        {
            //gameObject.SetActive(true);
            Vector3 screenEdge = m_vMainCamera.ViewportToWorldPoint(new Vector3(Mathf.Clamp(targetViewPortPosition.x, 0.04f, 0.96f), Mathf.Clamp(targetViewPortPosition.y, 0.07f, 0.93f), m_vMainCamera.nearClipPlane));
            //transform.position = new Vector3(screenEdge.x, screenEdge.y, 0);
            SmoothLerp(new Vector3(screenEdge.x, screenEdge.y, 0));

            //Vector3 direction = targetPosition - transform.position;
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        //}
        //}
    }

    private void SmoothLerp(Vector3 _vTargetPosition)
    {
        transform.position = Vector3.Lerp(transform.position, _vTargetPosition, Time.deltaTime * m_fSmoothLerpSpeed);
    }
}
