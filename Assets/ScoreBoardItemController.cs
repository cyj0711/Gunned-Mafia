using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardItemController : MonoBehaviour
{
    [SerializeField] private Text m_vNameText;
    [SerializeField] private Text m_vPingText;
    [SerializeField] private Text m_vKillText;
    [SerializeField] private Text m_vDeathText;
    [SerializeField] private Image m_vImage;

    private int m_iActorNumber;
    private int m_iPing;
    private string m_strName;
    private int m_iKill;
    private int m_iDeath;
    private E_PlayerRole m_ePlayerRole;
    private E_PlayerState m_ePlayerState;
    private bool m_bIsMasterClient;

    void Start()
    {
        
    }

    public void InitData(int _iActorNumber, string _strName)
    {
        m_iActorNumber = _iActorNumber;

        GameUIManager.I.CreateScoreBoardItem(m_iActorNumber, this);

        UpdateNameText(_strName);
        UpdatePingText(0);
        UpdateKillText(0);
        UpdateDeathText(0);
        UpdatePlayerRole(E_PlayerRole.None);
        UpdatePlayerState(E_PlayerState.Spectator);

    }

    void UpdateNameText(string _strName)
    {
        m_strName = _strName;
        m_vNameText.text = m_strName;
    }

    public void UpdatePingText(int _iPing)
    {
        m_iPing = _iPing;
        m_vPingText.text = m_iPing.ToString();
    }

    public void UpdateKillText(int _iKill)
    {
        m_iKill = _iKill;
        m_vKillText.text = m_iKill.ToString();
    }

    public void UpdateDeathText(int _iDeath)
    {
        m_iDeath = _iDeath;
        m_vDeathText.text = m_iDeath.ToString();
    }

    public void UpdatePlayerRole(E_PlayerRole _ePlayerRole)
    {
        m_ePlayerRole = _ePlayerRole;

        switch (m_ePlayerRole)
        {
            case E_PlayerRole.Detective:
                m_vImage.color = new Color(UIColor.Blue.r, UIColor.Blue.g, UIColor.Blue.b, 0.3f);
                break;
            case E_PlayerRole.Mafia:
                m_vImage.color = new Color(UIColor.Red.r, UIColor.Red.g, UIColor.Red.b, 0.3f);
                break;
            default:
                m_vImage.color = new Color(UIColor.Gray.r, UIColor.Gray.g, UIColor.Gray.b, 0.3f);
                break;
        }
    }

    public void UpdatePlayerState(E_PlayerState _ePlayerState)
    {
        m_ePlayerState = _ePlayerState;


        GameUIManager.I.SetScoreBoardItemParent(m_iActorNumber, m_ePlayerState);
    }
}
