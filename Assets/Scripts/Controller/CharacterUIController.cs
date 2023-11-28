using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIController : MonoBehaviour
{
    string m_sNickName;
    public string a_sNickName { set { m_sNickName = value; SetNickNameUI(); } }

    E_PlayerRole m_ePlayerRole;
    public E_PlayerRole a_ePlayerRole { set { m_ePlayerRole = value; SetRoleUI(); } }

    E_PlayerState m_ePlayerState;
    public E_PlayerState a_ePlayerState { set { m_ePlayerState = value; SetStateUI(); } }

    int m_iHealth;
    public int a_iHealth { set { m_iHealth = value; SetHealthUI(); } }

    int m_iPlayerActorNumber;
    public int a_iPlayerActorNumber { set { m_iPlayerActorNumber = value; } }

    [SerializeField]
    Text m_vNickNameText;
    [SerializeField]
    Text m_vHealthText;
    [SerializeField]
    GameObject m_vCanvasBody;
    [SerializeField] PhotonView m_vPhotonView;

    private int m_iLayerBlockAll;

    void Start()
    {
        if (!m_vPhotonView.IsMine)
        {
            if (GameManager.I.GetPlayerController(PhotonNetwork.LocalPlayer.ActorNumber).a_ePlayerState == E_PlayerState.Alive)
                m_vCanvasBody.SetActive(false);
        }

        m_iLayerBlockAll = 1 << LayerMask.NameToLayer("BlockAll");
    }

    public void SetUIData(string _sNickName, E_PlayerRole _ePlayerRole, int _iHealth)
    {
        m_sNickName = _sNickName;
        m_ePlayerRole = _ePlayerRole;
        m_iHealth = _iHealth;

        SetUI();
    }

    private void SetNickNameUI()
    {

        m_vNickNameText.text = m_sNickName;
    }

    public void SetRoleUI()
    {
        E_PlayerRole eLocalPlayerRole = GameManager.I.GetPlayerRole(PhotonNetwork.LocalPlayer.ActorNumber);
        switch (m_ePlayerRole)
        {
            case E_PlayerRole.Civil:
                m_vNickNameText.color = UIColor.Green;
                break;

            case E_PlayerRole.Mafia:
                m_vNickNameText.color = UIColor.Red;
                if (eLocalPlayerRole == E_PlayerRole.Civil || eLocalPlayerRole == E_PlayerRole.Detective)
                {
                    if (GameManager.I.GetPlayerController(PhotonNetwork.LocalPlayer.ActorNumber).a_ePlayerState == E_PlayerState.Alive)
                        m_vNickNameText.color = UIColor.Green;
                }
                break;

            case E_PlayerRole.Detective:
                m_vNickNameText.color = UIColor.Blue;
                break;

            default:
                m_vNickNameText.color = UIColor.Gray;
                break;
        }
    }

    private void SetHealthUI()
    {
        m_vHealthText.text = m_iHealth.ToString();
    }

    private void SetUI()
    {
        SetRoleUI();
        SetNickNameUI();
        SetHealthUI();
    }

    private void SetStateUI()
    {
        if(m_ePlayerState==E_PlayerState.Alive)
        {
            m_vHealthText.gameObject.SetActive(true);
        }
        else
        {
            m_vHealthText.gameObject.SetActive(false);
        }
    }

    //private void OnMouseEnter()
    //{
    //    if (!m_vPhotonView.IsMine)
    //    {
    //        //RaycastHit2D hit = Physics2D.Raycast(transform.position, GameManager.I.GetPlayerController().transform.position - transform.position, 10f, 1 << LayerMask.NameToLayer("BlockAll"));
    //        //if (hit.transform.gameObject == null)
    //        //{
    //        //    m_vCanvasBody.SetActive(true);
    //        //}

    //        m_vCanvasBody.SetActive(true);
    //    }
    //}

    // 다른 플레이어에게 마우스를 가져다대면 해당 플레이어의 이름과 체력이 표시된다.
    private void OnMouseOver()
    {
        SetCharacterUI(true);
    }

    private void OnMouseExit()
    {
        SetCharacterUI(false);
    }

    public void SetCharacterUI(bool _bIsOn)
    {
        if(_bIsOn)
        {
            // 로컬 플레이어가 죽은상태면 어차피 모든 이름이 표시되기 때문에 살아있을때만 OnMouseOver를 받는다
            if (!m_vPhotonView.IsMine && GameManager.I.GetPlayerController(PhotonNetwork.LocalPlayer.ActorNumber).a_ePlayerState == E_PlayerState.Alive)
            {
                // 로컬 플레이어와 타겟 플레이어 사이에 벽(layer가 BlockAll로 되어있는 오브젝트)이 있으면 시야가 가려져있으므로 표시하지 않는다.
                Vector3 vLocalPlayerPosition = GameManager.I.GetPlayerController().transform.position;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, vLocalPlayerPosition - transform.position, Vector2.Distance(vLocalPlayerPosition, transform.position), m_iLayerBlockAll);

                if (hit.transform != null)
                {
                    m_vCanvasBody.SetActive(false);
                }
                else
                {
                    m_vCanvasBody.SetActive(true);
                }
            }
        }
        else
        {
            if (!m_vPhotonView.IsMine)
            {
                if (GameManager.I.GetPlayerController(PhotonNetwork.LocalPlayer.ActorNumber).a_ePlayerState == E_PlayerState.Alive)
                    m_vCanvasBody.SetActive(false);
            }
        }
    }

    public void SetCanvasBodyActive(bool _bIsActive)
    {
        m_vCanvasBody.SetActive(_bIsActive);
    }
}
