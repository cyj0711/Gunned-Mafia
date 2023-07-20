using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    Dictionary<int, WeaponData> m_dicWeaponData = new Dictionary<int, WeaponData>();

    [SerializeField] GameObject m_vPlayerDeadBodyPrefab;
    public GameObject a_vPlayerDeadBodyPrefab { get { return m_vPlayerDeadBodyPrefab; } }

    [SerializeField] GameObject m_vLocationPingPrefab;
    public GameObject a_vLocationPingPrefab { get { return m_vLocationPingPrefab; } }

    void Start()
    {
        InitWeaponData();
    }

    private void InitWeaponData()
    {
        WeaponData[] arrayWeaponData = Resources.LoadAll<WeaponData>("Data/Weapon Data"); ;

        foreach (WeaponData weaponData in arrayWeaponData)
        {
            m_dicWeaponData.Add(weaponData.a_iWeaponId, weaponData);
        }
    }

    public WeaponData GetWeaponDataWithID(int id)
    {
        return m_dicWeaponData[id];
    }

}
