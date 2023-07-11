using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeadController : MonoBehaviour
{
    [SerializeField]
    Text m_vNickNameText;
    [SerializeField]
    GameObject m_vCanvasBody;

    int m_iVictimActorNumber;       // 사망 플레이어의 플레이어 번호
    E_PlayerRole m_ePlayerRole;   // 사망 플레이어의 역할
    int m_iKillerActorNumber;      // 범인의 플레이어 번호
    int m_iWeaponID;                // 범행 무기
    double m_dDeadTime;             // 사망 시각
    int m_iFirstFinderActorNumber;                // 처음으로 시체를 찾은 플레이어

    /* 두 bool형 모두 충족해야 시체조사 가능 */
    bool m_bIsCollisionEntered;     // 플레이어의 시체 근접 여부
    bool m_bIsMouseEntered;         // 마우스의 시체 근접 여부

    bool m_bIsSearchEnable;         // 시체 조사 가능 여부

    public int a_iVictimActorNumber { get { return m_iVictimActorNumber; } set { m_iVictimActorNumber = value; } }
    public E_PlayerRole a_ePlayerRole { get { return m_ePlayerRole; } set { m_ePlayerRole = value; } }
    public int a_iKillerActorNumber { get { return m_iKillerActorNumber; } set { m_iKillerActorNumber = value; } }
    public int a_iWeaponID { get { return m_iWeaponID; } set { m_iWeaponID = value; } }
    public double a_dDeadTime { get { return m_dDeadTime; } set { m_dDeadTime = value; } }
    public int a_iFirstFinderActorNumber { get { return m_iFirstFinderActorNumber; } set { m_iFirstFinderActorNumber = value; } }

    void Start()
    {
        m_bIsCollisionEntered = false;
        m_bIsMouseEntered = false;
        m_bIsSearchEnable = false;
        a_iFirstFinderActorNumber = -1;
    }

    private void Update()
    {
        if (m_bIsSearchEnable)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                GameUIManager.I.SetSearchText(m_iVictimActorNumber, m_ePlayerRole, m_iWeaponID, (int)(PhotonNetwork.Time - m_dDeadTime));
                GameUIManager.I.SetSearchPanelActive(true);
            }
        }
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

    private void OnMouseEnter()
    {
        m_bIsMouseEntered = true;

        if (m_bIsCollisionEntered == true)
            ActivateSearch(true);
    }

    private void OnMouseExit()
    {
        m_bIsMouseEntered = false;
        ActivateSearch(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController vPlayerController = collision.GetComponentInParent<PlayerController>();
        
        if(vPlayerController==null)
        {
            Debug.LogError("Body " + m_iVictimActorNumber + " is touched, but " + collision + "is null");
            return;
        }

        if (vPlayerController.a_vPhotonView.IsMine)
        {
            m_bIsCollisionEntered = true;

            if (m_bIsMouseEntered == true)
                ActivateSearch(true);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController vPlayerController = collision.GetComponentInParent<PlayerController>();

        if (vPlayerController == null)
        {
            Debug.LogError("Body " + m_iVictimActorNumber + " is touched, but " + collision + "is null");
            return;
        }

        if (vPlayerController.a_vPhotonView.IsMine)
        {
            m_bIsCollisionEntered = false;
            ActivateSearch(false);
        }

    }

    private void ActivateSearch(bool _bIsEnable)
    {
        m_vCanvasBody.SetActive(_bIsEnable);
        m_bIsSearchEnable = _bIsEnable;
    }
}
