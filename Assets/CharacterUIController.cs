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
    int m_iHealth;
    public int a_iHealth { set { m_iHealth = value; SetHealthUI(); } }

    int m_iPlayerActorNumber;
    public int a_iPlayerActorNumber { set { m_iPlayerActorNumber = value; } }

    [SerializeField]
    Text m_vNickNameText;
    [SerializeField]
    Text m_vHealthText;

    void Start()
    {

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

    private void SetRoleUI()
    {
        E_PlayerRole eLocalPlayerRole = GameManager.I.GetPlayerRole(PhotonNetwork.LocalPlayer.ActorNumber);
        switch (m_ePlayerRole)
        {
            case E_PlayerRole.Civil:
                m_vNickNameText.color = UIColor.Green;
                break;
            case E_PlayerRole.Mafia:
                if (eLocalPlayerRole == E_PlayerRole.Civil || eLocalPlayerRole == E_PlayerRole.Detective)
                    m_vNickNameText.color = UIColor.Green;
                else
                    m_vNickNameText.color = UIColor.Red;
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

    private void OnMouseEnter()
    {
        //m_vNickNameText.enabled = true;
        //m_vHealthText.enabled = true;
        Debug.Log("Mouse On");
    }
    private void OnMouseExit()
    {
        //m_vNickNameText.enabled = false;
        //m_vHealthText.enabled = false;

        Debug.Log("Mouse Out");
    }
}
