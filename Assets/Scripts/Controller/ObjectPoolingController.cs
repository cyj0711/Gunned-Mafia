using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// source by https://youtu.be/WVnuA6Jay8Q?si=xRhtPXNe45oKwLhe (고박사의 유니티 노트)
public class ObjectPoolingController
{
    // 메모리 풀로 관리되는 오브젝트 정보
    private class PoolItem
    {
        public bool isActive;   // "gameObject"의 활성화/비활성화 정보
        public GameObject gameObject;   // 화면에 보이는 실제 게임오브젝트
    }

    private int increaseCount = 5;  // 오브젝트가 부족할 때 Instantiate()로 추가 생성되는 오브젝트 개수
    private int maxCount;   // 현재 리스트에 등록되어 있는 오브젝트 개수
    private int activeCount;    // 현재 게임에 사용되고 있는(활성화) 오브젝트 개수

    private GameObject poolObject;  // 오브젝트 풀링에서 관리하는 게임의 오브젝트 프리팹
    private List<PoolItem> poolItemList;    // 관리되는 모든 오브젝트를 저장하는 리스트

    private Transform m_vObjectPoolingParent;

    public ObjectPoolingController(GameObject poolObject, Transform _vObjectPoolingParent)
    {
        m_vObjectPoolingParent = _vObjectPoolingParent;

        maxCount = 0;
        activeCount = 0;
        this.poolObject = poolObject;

        poolItemList = new List<PoolItem>();

        InstantiateObjects();
    }

    // increaseCount 단위로 오브젝트를 생성
    public void InstantiateObjects()
    {
        maxCount += increaseCount;

        for(int i=0;i<increaseCount;i++)
        {
            PoolItem poolItem = new PoolItem();

            poolItem.isActive = false;
            poolItem.gameObject = GameObject.Instantiate(poolObject);
            poolItem.gameObject.transform.SetParent(m_vObjectPoolingParent);
            poolItem.gameObject.SetActive(false);

            poolItemList.Add(poolItem);
        }
    }

    // 현재 관리중인(활성/비활성) 모든 오브젝트를 삭제
    // 씬 이동, 게임 종료 같은 상황에서 한번만 호출
    public void DestroyObjects()
    {
        if (poolItemList == null) return;

        int count = poolItemList.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject.Destroy(poolItemList[i].gameObject);
        }

        poolItemList.Clear();
    }

    // poolItemList에 저장되어 있는 오브젝트를 활성화해서 사용
    // 현재 모든 오브젝트가 사용중이면 InstantiateObjects()로 추가 생성
    public GameObject ActivatePoolItem()
    {
        if (poolItemList == null) return null;

        // 현재 생성해서 관리하는 모든 오브젝트 개수와 현재 활성화 상태인 오브젝트 개수 비교
        // 모든 오브젝트가 활성화 상태이면 새로운 오브젝트 필요
        if(maxCount==activeCount)
        {
            InstantiateObjects();
        }

        int count = poolItemList.Count;
        for(int i=0; i<count;i++)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.isActive==false)
            {
                activeCount++;

                poolItem.isActive = true;
                poolItem.gameObject.SetActive(true);

                return poolItem.gameObject;
            }
        }

        return null;
    }

    // 현재 사용이 완료된 오브젝트를 비활성화 상태로 설정
    public void DeactiveatePoolItem(GameObject removeObject)
    {
        if (poolItemList == null || removeObject == null) return;

        int count = poolItemList.Count;
        for(int i=0;i<count;i++)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.gameObject==removeObject)
            {
                activeCount--;

                poolItem.isActive = false;
                poolItem.gameObject.SetActive(false);

                return;
            }
        }
    }

    // 게임에서 사용중인 모든 오브젝트를 비활성화 상태로 설정
    public void DeactivateAllPoolItems()
    {
        if (poolItemList == null) return;

        int count = poolItemList.Count;
        for(int i=0;i<count;i++)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.gameObject!=null&&poolItem.isActive==true)
            {
                poolItem.isActive = false;
                poolItem.gameObject.SetActive(false);
            }
        }

        activeCount = 0;
    }
}
