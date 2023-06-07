using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanelManager : Singleton<GamePanelManager>
{
    [SerializeField] private Text m_vTimeText;
    [SerializeField] private Text m_vStatusText;
    [SerializeField] private Image m_vStatusImage;

    [SerializeField] private GameObject m_vAmmoArea;
    [SerializeField] private Text m_vAmmoText;
    [SerializeField] private Image m_vAmmoImage;

    void Start()
    {
        SetGameState();
        SetTimeText();
    }

    void Update()
    {
        //SetGameState();
        SetTimeText();
    }

    public void SetGameState()
    {
        E_GAMESTATE eGameState = GameManager.I.a_eGameState;

        switch (eGameState)
        {
            case E_GAMESTATE.Wait:
                m_vStatusImage.color = UIColor.Gray;
                m_vStatusText.text = eGameState.ToString();
                m_vTimeText.enabled = false;
                break;
            case E_GAMESTATE.Play:
                SetPlayingState();
                m_vTimeText.enabled = true;
                break;
            default:
                m_vStatusImage.color = UIColor.Gray;
                m_vStatusText.text = eGameState.ToString();
                m_vTimeText.enabled = true;
                break;
        }

    }

    private void SetPlayingState()
    {
        E_PlayerRole ePlayerRole = GameManager.I.GetPlayerRole();

        switch (ePlayerRole)
        {
            case E_PlayerRole.Civil:
                m_vStatusImage.color = UIColor.Green;
                break;
            case E_PlayerRole.Mafia:
                m_vStatusImage.color = UIColor.Red;
                break;
            case E_PlayerRole.Detective:
                m_vStatusImage.color = UIColor.Blue;
                break;
            default:
                m_vStatusImage.color = UIColor.Gray;
                break;
        }
        m_vStatusText.text = ePlayerRole.ToString();
    }

    private void SetTimeText()
    {
        int iTime = (int)GameManager.I.GetTime();
        m_vTimeText.text = (iTime / 60).ToString("D2") + ":" + (iTime % 60).ToString("D2");
    }

    public void SetAmmo(int iAmmoCapacity, int iCurrentAmmo, int iRemainAmmo)
    {
        m_vAmmoImage.fillAmount = (float)(iCurrentAmmo / (float)iAmmoCapacity);
        m_vAmmoText.text = iCurrentAmmo.ToString("D2") + " / " + iRemainAmmo.ToString("D2");
    }

    public void SetAmmoActive(bool bActive)
    {
        m_vAmmoArea.SetActive(bActive);
    }
}
