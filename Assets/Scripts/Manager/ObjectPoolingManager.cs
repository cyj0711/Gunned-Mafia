using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    [SerializeField]
    private GameObject m_vBulletPrefab;
    [SerializeField]
    private Transform m_vObjectPoolingParent;

    private ObjectPoolingController m_vBulletObjectPool;
    public ObjectPoolingController a_vBulletObjectPool { get => m_vBulletObjectPool; }

    private void Start()
    {
        m_vBulletObjectPool = new ObjectPoolingController(m_vBulletPrefab, m_vObjectPoolingParent);
    }
}
