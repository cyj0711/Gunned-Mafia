using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDead : MonoBehaviour
{
    int m_iVictimActorNumber;       // 사망 유저의 플레이어 번호
    E_PlayerRole m_ePlayerRole;   // 사망 유저의 역할
    int m_iKillerActorNumber;      // 범인의 플레이어 번호
    int m_iWeaponID;                // 범행 무기
    double m_dDeadTime;             // 사망 시각

    public int a_iVictimActorNumber { get { return m_iVictimActorNumber; } set { m_iVictimActorNumber = value; } }
    public E_PlayerRole a_ePlayerRole { get { return m_ePlayerRole; } set { m_ePlayerRole = value; } }
    public int a_iKillerActorNumber { get { return m_iKillerActorNumber; } set { m_iKillerActorNumber = value; } }
    public int a_iWeaponID { get { return m_iWeaponID; } set { m_iWeaponID = value; } }
    public double a_dDeadTime { get { return m_dDeadTime; } set { m_dDeadTime = value; } }

    void Start()
    {

    }

    public void InitData(int _iVictimActorNumber, E_PlayerRole _ePlayerRole, int _iKillerActorNumber, int _iWeaponID, double _dDeadTime)
    {
        a_iVictimActorNumber = _iVictimActorNumber;
        a_ePlayerRole = _ePlayerRole;
        a_iKillerActorNumber = _iKillerActorNumber;
        a_iWeaponID = _iWeaponID;
        a_dDeadTime = _dDeadTime;

        //Debug.Log(a_iVictimActorNumber + " is killed by " + a_iKillerActorNumber + " with " + DataManager.I.GetWeaponDataWithID(a_iWeaponID).a_strWeaponName + " at " + a_dDeadTime + ". He is a" + a_ePlayerRole);
    }
}
