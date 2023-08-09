using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeadController : MonoBehaviour, IPunInstantiateMagicCallback
{
    [SerializeField]
    Text m_vNickNameText;
    [SerializeField]
    GameObject m_vCanvasBody;

    [SerializeField]
    private PhotonView m_vPhotonView;
    public PhotonView a_vPhotonView { get { return m_vPhotonView; } }

    int m_iVictimActorNumber;       // 사망 플레이어의 플레이어 번호
    E_PlayerRole m_ePlayerRole;     // 사망 플레이어의 역할
    int m_iKillerActorNumber;       // 범인의 플레이어 번호
    int m_iWeaponID;                // 범행 무기
    double m_dDeadTime;             // 사망 시각
    int m_iFirstWitnessActorNumber; // 처음으로 시체를 찾은 플레이어
    string m_strVictimNickName;     // 사망 플레이어 닉네임(사망 후 나가면 photon을 통한 닉네임 참조가 안되므로 미리저장함)
    int m_iRemainTimeDNA;           // 범인의 DNA가 남아있는 시간 (초 단위)

    /* 두 bool형 모두 충족해야 시체조사 가능 */
    bool m_bIsCollisionEntered;     // 플레이어의 시체 근접 여부
    bool m_bIsMouseEntered;         // 마우스의 시체 근접 여부

    bool m_bIsSearchEnable;         // 시체 조사 가능 여부

    public int a_iVictimActorNumber { get { return m_iVictimActorNumber; } }
    public void SetVictimActorNumber(int _iValue) { m_iVictimActorNumber = _iValue; }
    public E_PlayerRole a_ePlayerRole { get { return m_ePlayerRole; } }
    public int a_iKillerActorNumber { get { return m_iKillerActorNumber; } }
    public int a_iWeaponID { get { return m_iWeaponID; } }
    public double a_dDeadTime { get { return m_dDeadTime; } }
    public int a_iFirstWitnessActorNumber { get { return m_iFirstWitnessActorNumber; } }
    public void SetFirstWitnessActorNumber(int _iValue) { m_iFirstWitnessActorNumber = _iValue; }
    public int a_iRemainTimeDNA { get { return m_iRemainTimeDNA; } }


    void Start()
    {
        m_bIsCollisionEntered = false;
        m_bIsMouseEntered = false;
        m_bIsSearchEnable = false;
        m_iFirstWitnessActorNumber = -1;
    }

    private void Update()
    {
        if (m_bIsSearchEnable)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                if (m_iFirstWitnessActorNumber == -1)   // 최초 발견자인지 확인(알림 띄우는 용도)
                {
                    GameManager.I.CheckIsPlayerFirstWitness(m_iVictimActorNumber, PhotonNetwork.LocalPlayer.ActorNumber);   // 최초 발견자가 맞다면 PlayerDeadController.cs의 NotifyDead 함수 호출
                }

                UISearchManager.I.SetSearchText(m_iVictimActorNumber, m_iKillerActorNumber, m_strVictimNickName, m_ePlayerRole, m_iWeaponID, (int)(PhotonNetwork.Time - m_dDeadTime), Mathf.Max(m_iRemainTimeDNA - (int)(PhotonNetwork.Time - m_dDeadTime), 0));
                UISearchManager.I.SetSearchPanelActive(true);
            }
        }
    }

    public void InitData(int _iVictimActorNumber, E_PlayerRole _ePlayerRole, int _iKillerActorNumber, int _iWeaponID, double _dDeadTime, float _fKillerDistance, string _strVictimNickName)
    {
        m_iVictimActorNumber = _iVictimActorNumber;
        m_ePlayerRole = _ePlayerRole;
        m_iKillerActorNumber = _iKillerActorNumber;
        m_iWeaponID = _iWeaponID;
        m_dDeadTime = _dDeadTime;
        m_strVictimNickName = _strVictimNickName;

        m_iRemainTimeDNA = SetRemainTimeDNA(_fKillerDistance);

        //Debug.Log(a_iVictimActorNumber + " is killed by " + a_iKillerActorNumber + " with " + DataManager.I.GetWeaponDataWithID(a_iWeaponID).a_strWeaponName + " at " + a_dDeadTime + ". He is a" + a_ePlayerRole);
    }

    // 사망 당시 피해자와 범인 사이의 거리를 계산해 범인의 dna 잔재 시간 계산
    private int SetRemainTimeDNA(float _fKillerDistance)
    {
        float fRoundedDistance = (float)System.Math.Round(_fKillerDistance, 2);
        return Mathf.Max((int)((120 / fRoundedDistance) - (fRoundedDistance * fRoundedDistance)), 0);
    }

    // 시체에 가까이가서 마우스를 가져다 대면 조사가 가능하다.
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

        if (vPlayerController == null)
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

    public void NotifyDead(int _iFirstWitness)
    {
        UIGameManager.I.CreateNotificationToAll(PhotonNetwork.CurrentRoom.GetPlayer(_iFirstWitness).NickName + " found the body of " + m_strVictimNickName + ". He was " + m_ePlayerRole + "!");

        m_vPhotonView.RPC(nameof(SetBodyNameRPC), RpcTarget.AllBuffered, _iFirstWitness);

        PlayerController vPlayerController = GameManager.I.GetPlayerController(a_iVictimActorNumber);
        if (vPlayerController == null)
            return;

        vPlayerController.a_ePlayerState = E_PlayerState.Dead;

    }

    [PunRPC]
    private void SetBodyNameRPC(int _iFirstWitness)
    {
        m_iFirstWitnessActorNumber = _iFirstWitness;
        m_vNickNameText.color = Color.white;
        m_vNickNameText.text = m_strVictimNickName;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] vInstantiationData = info.photonView.InstantiationData;

        if (vInstantiationData != null && vInstantiationData.Length == 6)   // vInstantiationData 내용은 MapManager.cs 의 SpawnPlayerDeadBody() 함수 참고
        {
            InitData((int)vInstantiationData[0], GameManager.I.GetPlayerRole((int)vInstantiationData[0]), (int)vInstantiationData[1],
                (int)vInstantiationData[2], (double)vInstantiationData[3], (float)vInstantiationData[4], (string)vInstantiationData[5]);

            MapManager.I.AddPlayerDeadInfo((int)vInstantiationData[0], this);

            // 사망한 유저의 위치가 탐정에게 표시중이었다면, 표시 위치를 해당 유저의 시체로 바꾼다.
            if(UISearchManager.I.a_dicPlayerLocationPing.ContainsKey(m_iVictimActorNumber))
            {
                UISearchManager.I.ChangeTrackedPlayerToBody(m_iVictimActorNumber);
            }
        }

    }
}
