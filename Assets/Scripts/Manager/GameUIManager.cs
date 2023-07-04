using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : Singleton<GameUIManager>
{
    [SerializeField] private Text m_vTimeText;
    [SerializeField] private Text m_vStatusText;
    [SerializeField] private Image m_vStatusImage;

    [SerializeField] private GameObject m_vAmmoArea;
    [SerializeField] private Text m_vAmmoText;
    [SerializeField] private Image m_vAmmoImage;

    [SerializeField] private GameObject m_vScoreBoardPanelObject;
    [SerializeField] private Scrollbar m_vScoreBoardScrollBar;
    [SerializeField] private GameObject m_vScoreBoardItemPrefab;

    [SerializeField] private Transform m_vAlivePlayerListTransform;
    [SerializeField] private Transform m_vMissingPlayerListTransform;
    [SerializeField] private Transform m_vDeadPlayerListTransform;
    [SerializeField] private Transform m_vSpectatorPlayerListTransform;

    private Dictionary<int, ScoreBoardItemController> m_dicScoreBoardItems = new Dictionary<int, ScoreBoardItemController>();

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

    public void ShowScoreBoard(bool bActive)
    {
        m_vScoreBoardPanelObject.SetActive(bActive);
        //m_vScoreBoardScrollBar.value = 1.0f;
    }

    public void CreateScoreBoardItem(int _iActorNumber, ScoreBoardItemController _vScoreBoardItem)
    {
        if (!m_dicScoreBoardItems.ContainsKey(_iActorNumber))
        {
            m_dicScoreBoardItems.Add(_iActorNumber, _vScoreBoardItem);
        }
        else
        {
            Debug.LogError("Tried to Add ScoreBoard, but player " + _iActorNumber + " alredy exist");
            return;
        }
    }

    public void RemoveScoreBoardItem(int _iActorNumber)
    {
        if (m_dicScoreBoardItems.ContainsKey(_iActorNumber))
        {
            ScoreBoardItemController vScoreBoardItemController = m_dicScoreBoardItems[_iActorNumber];
            m_dicScoreBoardItems.Remove(_iActorNumber);
            Destroy(vScoreBoardItemController.gameObject);
        }
    }

    public void SetScoreBoardItemParent(int _iActorNumber, E_PlayerState _ePlayerState)
    {
        switch (_ePlayerState)
        {
            case E_PlayerState.Alive:
                m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vAlivePlayerListTransform);
                break;
            case E_PlayerState.Missing:
                m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vMissingPlayerListTransform);
                break;
            case E_PlayerState.Dead:
                m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vDeadPlayerListTransform);
                break;
            case E_PlayerState.Spectator:
                m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vSpectatorPlayerListTransform);
                break;
        }
        m_dicScoreBoardItems[_iActorNumber].transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
